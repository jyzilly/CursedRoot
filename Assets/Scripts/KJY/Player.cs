using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    //간단한 HP바, 실제 없음, 플레이어 데미지 관리용
    public float CurrentHp
    {
        get => currentHp;
        set
        {
            currentHp = Mathf.Clamp(value, 0f, maxHp);
            //hpBar.UpdateHpBar(maxHp, currentHp);

            if (currentHp <= 0f)
            {
                Die();
            }
        }
    }


    [Header("Hp Settings")]
    [SerializeField] private float maxHp = 100f;

    [Header("Shake Settings")]
    [SerializeField] private GameObject hitEffect1;
    [SerializeField] private GameObject hitEffect2;
    [SerializeField] private CameraMovement cam;

    [SerializeField] private Creature2SceneManager creature2scenemanager;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private GameObject DieUI;
    private float currentHp;
    private Animator animator;

    //대사 출력 담당 ->  다혜씨 코드
    [SerializeField] private DialogueManager dialogueManager; 
    public bool isInside = false;


    private void Start()
    {
        animator = GetComponent<Animator>();
        //체력 초기화 및 HP바 초기화
        CurrentHp = maxHp;
    }

    public void TakeDamage(float damage)
    {
        CurrentHp -= damage;
    }

    //피격효과
    public IEnumerator PlayerHitEffect()
    {
        animator.SetBool("hit", true);
        hitEffect1.SetActive(true);
        hitEffect2.SetActive(true);
        //일정시간 대기
        yield return new WaitForSeconds(0.8f);
        //비활성화
        animator.SetBool("hit", false);
        hitEffect1.SetActive(false);
        hitEffect2.SetActive(false);
    }

    public void Heal(float amount)
    {
        CurrentHp += amount;
    }


    private void Die()
    {
        pm.isMovable = false;
        //Debug.Log("플레이어 사망!");
        playerController.enabled = false;


        StopAllCoroutines();
        //여기서 사망 애니메이션, 게임 오버 처리 등 넣기
        animator.SetBool("die", true);

        DieUI.SetActive(true);

        //2초 후 씬 전환 실행 -> 다혜씨 코드
        Invoke("DelayToDie", 3f);

    }

    // -> 다혜씨 코드
    private void DelayToDie()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == "Creature2Map")
        {
            creature2scenemanager.RestartScene();
        }
        else if (currentScene == "Boss 1")
        {
            SceneLoad.LoadSceneWithLoading("Boss 1");
        }
        else if (currentScene == "Creature1Map")
        {
            SceneLoad.LoadSceneWithLoading("Creature1Map");
        }
    }

    // -> 다혜씨 코드
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bossDoor"))
        {
            Debug.Log("보스문 충돌");
            //m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.);

            //StartCoroutine(PlayDialogue("goToBossDoor"));
            dialogueManager.PlayDialogue("goToBossDoor", "player");
        }

        if (other.CompareTag("DontMonster"))
            isInside = true;

    }

    // -> 다혜씨 코드
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("DontMonster"))
            isInside = false;
    }

    //돌멩이 타격 -> 다혜씨 코드
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Rock"))
        {
            TakeDamage(5);
            StartCoroutine(PlayerHitEffect());
        }
    }

}
