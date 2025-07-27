using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemManager : MonoBehaviour
{
    [Header("Hand & Pickup Settings")]
    //손 위치
    [SerializeField] private Transform handTransform;
    //레이어설정
    [SerializeField] private LayerMask interacterLayer;
    [SerializeField] private LayerMask pickableLayer;
    [SerializeField] private LayerMask obstacleLayer;
    //전방 원뿔 반경
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float detectionRadius = 0.5f;
    //전방 원뿔 절반 각도
    //[SerializeField] private float panAngle = 30f;
    //flash
    [SerializeField] private FlashManager flashManager;
    [SerializeField] private ObanggiUIManager obanggiUIManager;
    [SerializeField] private DialogueManager dialogueManager;

    [SerializeField] private AudioSource itemPickUp;

    [SerializeField] private GameObject howToUse;

    [Header("Key UI Settings")]
    [SerializeField] private GameObject EKeyUI;

    [Header("Gimmick UI Settings")]
    [SerializeField] private GameObject noteUI;

    [Header("Note UI Settings")]
    public TextMeshProUGUI noteUIText;


    //상호작용 키
    private KeyCode interactKey = KeyCode.E;
    private Camera mainCamera;
    //손에 든 아이템
    public GameObject currentItem;
    //현재 근처 대상
    private GameObject nearbyInteractable;
    //픽업 대상
    private GameObject pickableTarget;
    //애니메이션
    private Animator animator;

    //SphereCast가 맞은 마지막 위치를 저장할 변수
    private Vector3 lastHitPoint;
    // public PlayerTriggerMAnager PTM;

    private void Start()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        if (EKeyUI) EKeyUI.SetActive(false);
    }

    private void Update()
    {

        //카메라 전방 사
        DetectTargetInView();

        //UI 토글 & 위치 업데이트
        bool showUI = (nearbyInteractable != null) || (pickableTarget != null);
        if (EKeyUI.activeSelf != showUI)
            EKeyUI.SetActive(showUI);

        if (showUI)
        {
            //저장된 마지막 충돌 지점을 UI 위치로 사용
            Vector3 worldPos = lastHitPoint;
            EKeyUI.transform.position = mainCamera.WorldToScreenPoint(worldPos);
        }

        if (Input.GetKeyDown(interactKey))
        {
            //여기서 상호작용 필요하는 오브젝트들 기능추가, 형식 비슷함, 읽기기믹하고 픽업기믹 따라서 사용해야 하는 부분 여기서 적으면 됨
            
            //Debug.Log("mainItem : " + MainItemManager.Instance.mainItem);

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Creature2Map")
            {
                howToUse.SetActive(false);
            }

            //E키를 누르면 무조건 끄는 것
            if (noteUI.activeSelf)
            {
                noteUI.SetActive(false);
                return;
            }

            //읽기 기믹
            if (nearbyInteractable != null)
            {
                //Debug.Log("Pick");
                animator.SetTrigger("pickStand");

                HandleCreature2MapInteractions();

                //쪽지들은 전부 note태그
                //각 쪽지마다 noteText스크립트 달고 해당하는 내용은 각각 수정
                //상호작용 된 쪽지의 컴포넌트 속 Text를 가져와서 글자를 띄움
                if (nearbyInteractable.CompareTag("Note"))
                {
                    NoteText content = nearbyInteractable.GetComponent<NoteText>();
                    if (content != null)
                    {
                        noteUIText.text = content.noteText;
                    }

                    noteUI.SetActive(!noteUI.activeSelf);
                    m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.paperOn);
                    return;
                }

                //크리처2맵에서 석상 불켜기 할 때 필요한 태그
                if (nearbyInteractable.CompareTag("Statue"))
                {
                    Statue statue = nearbyInteractable.GetComponent<Statue>();
                    if (statue != null)
                    {
                        statue.LightFire();
                    }
                    return;
                }

                //IInteractable
                if (nearbyInteractable.TryGetComponent<IInteractable>(out var inter))
                {
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Creature2Map")
                    {
                        Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.Pwall);
                    }
                    inter.OnInteract(currentItem);
                    return;
                }

                //장독대
                if (nearbyInteractable.CompareTag("Jar"))
                {
                    Jar breaker = nearbyInteractable.GetComponent<Jar>();
                    if (breaker != null)
                    {
                        breaker.BreakJar();
                    }
                    return;
                }

                if (nearbyInteractable.CompareTag("Door"))
                {
                    Door door = nearbyInteractable.GetComponent<Door>();
                    if (door != null)
                    {
                        door.TheDorrControl();
                    }
                    return;
                }

                if (nearbyInteractable.CompareTag("spiderEgg"))
                {
                    BreakOnXKey breakOnXKey = nearbyInteractable.GetComponentInParent<BreakOnXKey>();
                    if (breakOnXKey != null)
                    {
                        breakOnXKey.ProcessEggBreak();
                    }
                    return;
                }

                if (nearbyInteractable.CompareTag("Wall"))
                {
                    BrokenWall brokenWall = nearbyInteractable.GetComponentInParent<BrokenWall>();
                    if (brokenWall != null)
                    {
                        brokenWall.BreakOnce();
                    }
                    return;
                }

                if (nearbyInteractable.CompareTag("UnBrokenJar"))
                {
                    m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.unBrokenJar);
                    dialogueManager.PlayDialogue("interactUnBrokenJar", "player");
                    return;
                }
                if (nearbyInteractable.CompareTag("Well"))
                {
                    dialogueManager.PlayDialogue("interactWell", "player");
                    return;
                }
                if (nearbyInteractable.CompareTag("BossDoor"))
                {
                    GameSaveManager.SaveProgress("Boss");
                    SceneLoad.LoadSceneWithLoading("Boss");
                    //SceneManager.LoadScene("Boss");
                    return;
                }


                //메인 아이템이면
                if (nearbyInteractable.CompareTag("MainItem"))
                {
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Creature2Map")
                    {
                        Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.PhorrorEffect);
                        dialogueManager.PlayDialogue("inPrison2", "player");
                    }
                    MainItemManager.Instance.mainItem += 1;
                    Destroy(nearbyInteractable);
                    return;
                }
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Boss")
                {
                    if (nearbyInteractable.CompareTag("Obanggi"))
                    {
                        obanggiUIManager.mainUIPanel.SetActive(true);
                        return;
                    }

                    if (nearbyInteractable.CompareTag("ObanggiEast"))
                    {
                        obanggiUIManager.blueEast = true;
                        Destroy(nearbyInteractable);
                        return;
                    }

                    if (nearbyInteractable.CompareTag("ObanggiWest"))
                    {
                        obanggiUIManager.whiteWest = true;
                        Destroy(nearbyInteractable);
                        return;
                    }

                    if (nearbyInteractable.CompareTag("ObanggiSouth"))
                    {
                        obanggiUIManager.redSouth = true;
                        Destroy(nearbyInteractable);
                        return;
                    }

                    if (nearbyInteractable.CompareTag("ObanggiNorth"))
                    {
                        obanggiUIManager.blackNorth = true;
                        Destroy(nearbyInteractable);
                        return;
                    }
                }



            }

            //픽업
            if (pickableTarget != null)
            {
                //itemPickUp.Play();
                Vector3 directionToTarget = pickableTarget.transform.position - transform.position;
                float verticalOffset = directionToTarget.y;

                if (verticalOffset < 0.5f) //바닥에 있을 경우
                {
                    animator.SetTrigger("pickSit");
                }
                else //앞에 있을 경우
                {
                    animator.SetTrigger("pickStand");
                }

                return;

            }
            //후레쉬 On/Off 전용 처리
            if (currentItem != null && currentItem.CompareTag("Flashlight"))
            {
                flashManager?.Toggle();
            }

        }

    }


    private void DetectTargetInView()
    {
        nearbyInteractable = null;
        pickableTarget = null;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        LayerMask combinedLayer = interacterLayer | pickableLayer;

        if (Physics.SphereCast(ray.origin, detectionRadius, ray.direction, out hit, detectRange, combinedLayer))
        {

            Vector3 directionToTarget = hit.point - ray.origin;
            float distanceToTarget = hit.distance;

            if (!Physics.Raycast(ray.origin, directionToTarget.normalized, distanceToTarget, obstacleLayer))
            {
                //장애물이 없는 경우에만 상호작용 대상을 설정
                lastHitPoint = hit.point;
                GameObject hitObject = hit.collider.gameObject;
                int hitLayer = hitObject.layer;

                if ((pickableLayer.value & (1 << hitLayer)) != 0)
                {
                    if (currentItem == null || (hitObject != currentItem && !hitObject.transform.IsChildOf(handTransform)))
                    {
                        pickableTarget = hitObject;
                    }
                }
                else if ((interacterLayer.value & (1 << hitLayer)) != 0)
                {
                    nearbyInteractable = hitObject;
                }
            }
        }
    }


    //크리처2맵에서만 베터리가 있음, 따라 빼서 사용 더 효율적
    private bool HandleCreature2MapInteractions()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Creature2Map")
            return false;

        if (nearbyInteractable.CompareTag("Battery"))
        {
            if (flashManager != null)
            {
                flashManager.RefillBattery(30f);
            }
            Destroy(nearbyInteractable);
            return true;
        }

        return false;
    }

    //애니메이션 이벤트에 추가
    public void OnPickupAnimationEnd()
    {
        if (pickableTarget != null)
        {
            PickupItem(pickableTarget);
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void PickupItem(GameObject item)
    {
        if (currentItem != null)
            DropCurrentItem();

        itemPickUp.Play();

        currentItem = item;
        if (item.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        //후레수이일 경우 키고, 손에 있다는 상태로 바꿔주고
        if (item.CompareTag("Flashlight"))
        {
            flashManager.flashUIParticleSystem.Stop();
            flashManager.TurnOn();
            flashManager.SetHeld(true);
        }

        item.transform.SetParent(handTransform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        //손에 있는 아이템을 알려주고 HeldItem 레이어를 부여
        SetLayerRecursively(currentItem, LayerMask.NameToLayer("HeldItem"));
    }

    //공격당할 때 손에 있는 아이템을 떨어트리기, 외부에서 공격당하거나 하는 코드에서 추가 해야 함
    public void DropCurrentItem()
    {
        if (currentItem == null) return;


        if (currentItem.CompareTag("Flashlight"))
        {
            flashManager?.SetHeld(false);
        }

        var item = currentItem;
        item.transform.SetParent(null);
        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }

        int originalLayer = LayerMask.NameToLayer("Item");
        SetLayerRecursively(item, originalLayer);

        currentItem = null;
    }

    //상호작용 끝나고 손에 있는 아이템을 파괴할 때 외부에서 호출
    public void DestroyCurrentItem()
    {
        if (currentItem == null) return;

        Destroy(currentItem);

        currentItem = null;
    }


    //디버그
    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }


        Vector3 rayOrigin = mainCamera.transform.position;
        Vector3 rayDirection = mainCamera.transform.forward;

        RaycastHit hit;
        LayerMask combinedLayer = interacterLayer | pickableLayer;

        //수정된 시작점과 방향으로 SphereCast를 실행
        if (Physics.SphereCast(rayOrigin, detectionRadius, rayDirection, out hit, detectRange, combinedLayer))
        {
            //맞았을 때 -> 녹색으로 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rayOrigin, detectionRadius);
            Gizmos.DrawLine(rayOrigin, hit.point);
            Gizmos.DrawWireSphere(hit.point, detectionRadius);
        }
        else
        {
            // 맞지 않았을 때: 빨간색으로 표시
            Gizmos.color = Color.red;
            Vector3 endPoint = rayOrigin + rayDirection * detectRange;
            Gizmos.DrawWireSphere(rayOrigin, detectionRadius);
            Gizmos.DrawLine(rayOrigin, endPoint);
            Gizmos.DrawWireSphere(endPoint, detectionRadius);
        }
    }

}