using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


//3인칭으로 위치를 설정하고,고정 그 위치에 따라가게 한다. 플레이어과 일정한 거리를 유지하며, 따라가는 거 어색하지 않게, 마우스의 입력따라 카메라 이동
//오토포커스, 일정한 시간내에 마우스 입력이 없으면 자동 중간으로 가기 (안 쓰기로 함)

public class CameraMovement : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Follow Settings")]
    [SerializeField] private Transform objectTofollow;
    [SerializeField] private float originalCamLocalY;

    [Header("Mouse Look Settings")]
    [SerializeField] private float sensitivity = 100f;
    [SerializeField] private float clampAngle = 70f;

    [Header("Camera Collision & Zoom")]
    [SerializeField] private GameObject realCam;
    [SerializeField] private Transform realCamera;
    //카메라 로컬 방향
    [SerializeField] private Vector3 dirNormalized; 
    //최대거리 -> 계속 유지하는 거리, 최소거리 -> 벽이나 충돌할 때 거의 1인칭으로 바꾸는 거리로 전환
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    //충돌 보정 거리 -> 최종 카메라 거리
    [SerializeField] private float currentCollisionDistance; 
    //보간 속도
    [SerializeField] private float smoothness = 10f;
    //충돌 시 카메라가 벽에서 살짝 떨어지게 하는 값
    [SerializeField] private float collisionOffset = 0.2f; 
    [SerializeField] private LayerMask collisionLayer;

    //SphereCast에 사용할 카메라 구체 반경
    [SerializeField] private float cameraRadius = 0.2f;

    [Header("Auto Settings")]
    //사용,미사용설정
    [SerializeField] private bool useAutoAlign = false;
    //작은 입력 값을 무시
    [SerializeField] private float noMouseInputThreshold = 0.01f;
    //마우스 입력 없을 시 자동 정렬 시작 시간
    [SerializeField] private float timeBeforeAutoAlign = 1.0f;
    [SerializeField] private float autoAlignSpeed = 3f;
    [SerializeField] private float defaultAutoAlignRotX = 10f;
    //크리처2맵에만 사용
    [SerializeField] private string autoAlignSceneName = "Creature2Map";

    private float timeSinceLastMouseInput = 0f;
    private bool isAutoAligning = false;
    //시야 고정 토글 사용, 미사용설정
    private bool isViewLocked = false;
    private float rotX;
    private float rotY;


    private void Start()
    {
        if (objectTofollow != null)
        {
            transform.localPosition = objectTofollow.transform.position;
            rotY = objectTofollow.transform.eulerAngles.y;
            rotX = defaultAutoAlignRotX;
        }
        else
        {
            rotX = transform.localRotation.eulerAngles.x;
            rotY = transform.localRotation.eulerAngles.y;
        }

        if (realCamera != null)
        {
            //realCamera의 초기 로컬 위치를 기준으로 dirNormalized 설정
            dirNormalized = realCamera.localPosition.normalized;
            //초기 카메라 거리를 currentCollisionDistance에 저장
            currentCollisionDistance = Vector3.Distance(transform.position, realCamera.position);

        }
        else
        {
            Debug.LogError("Camera Wrong");
        }

        //초기 currentCollisionDistance를 maxDistance로 설정
        currentCollisionDistance = maxDistance;
    }

    private void Update()
    {
        //Q키로 시야 고정/해제 토글
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isViewLocked = !isViewLocked;
            if (isViewLocked)
            {
                isAutoAligning = false;
                timeSinceLastMouseInput = 0f;
            }
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        //시야가 잠기지 않았을 때만 마우스로 회전
        if (!isViewLocked)
        {
            float mouseXInput = Input.GetAxis("Mouse X");
            float mouseYInput = Input.GetAxis("Mouse Y");

            //마우스 입력 감지
            if (Mathf.Abs(mouseXInput) > noMouseInputThreshold || Mathf.Abs(mouseYInput) > noMouseInputThreshold)
            {
                //마우스로 회전
                rotX += -mouseYInput * sensitivity;// * Time.deltaTime;
                rotY += mouseXInput * sensitivity;// * Time.deltaTime;
                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

                //마우스 회전중 리셋 & 중지
                timeSinceLastMouseInput = 0f;
                isAutoAligning = false;
            }
            else
            {
                //마우스 비활성시 타이머시작
                if (useAutoAlign)
                {
                    timeSinceLastMouseInput += Time.deltaTime;
                }
            }

            //자동 정렬 기능 사용하는 경우
            if (useAutoAlign)
            {
                //씬 체크
                if (currentSceneName == autoAlignSceneName)
                {
                    //플레이어이동 체크
                    if (playerMovement != null && !playerMovement.IsMoving)
                    {
                        //입력여부 체크
                        if (timeSinceLastMouseInput >= timeBeforeAutoAlign && !isAutoAligning)
                        {
                            isAutoAligning = true;
                        }
                    }
                    else
                    {
                        isAutoAligning = false;
                        timeSinceLastMouseInput = 0f;
                    }
                }
            }
            else //자동 정렬을 사용하지 않는 경우, 끄기
            {
                isAutoAligning = false;
            }

            if (isAutoAligning)
            {
                //원래저장했던 값 -> 타겟값으로 설정
                float targetRotY = objectTofollow.transform.eulerAngles.y;
                float targetRotX = defaultAutoAlignRotX;

                //Lerp 부드럽게 이동
                rotY = Mathf.LerpAngle(rotY, targetRotY, autoAlignSpeed * Time.deltaTime);
                rotX = Mathf.Lerp(rotX, targetRotX, autoAlignSpeed * Time.deltaTime);

                //DeltaAngle은 두 각도 사이의 가장 짧은 거리를 계산
                if (Mathf.DeltaAngle(rotY, targetRotY) < 0.5f && Mathf.Abs(rotX - targetRotX) < 0.5f)
                {
                    rotY = targetRotY;
                    rotX = targetRotX;
                    isAutoAligning = false;
                    //정렬 후 다시 리셋
                    timeSinceLastMouseInput = 0f;
                }
            }
        }

        Quaternion camPivotRotation = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = camPivotRotation;
    }

    private void LateUpdate()
    {
        if (objectTofollow == null || realCamera == null || playerMovement == null) return;

        //카메라 피봇을 플레이어 위치에 고정
        transform.position = objectTofollow.position;

        //카메라의 목표 월드 위치 계산(최대 거리 기준)
        Vector3 desiredCameraWorldPos = transform.position + transform.rotation * (dirNormalized * maxDistance);

        RaycastHit hit;
        //SphereCast를 사용하여 충돌 감지
        if (Physics.SphereCast(transform.position, cameraRadius, (desiredCameraWorldPos - transform.position).normalized, out hit, maxDistance, collisionLayer))
        {
            //충돌이 감지되면 충돌 지점에서 살짝 뒤로 물러난 거리를 목표 거리로 설정
            currentCollisionDistance = Mathf.Clamp(hit.distance - collisionOffset, minDistance, maxDistance);
        }
        else
        {
            //충돌이 없으면 최대 거리로 설정
            currentCollisionDistance = maxDistance;
        }

        Vector3 targetLocalPos = dirNormalized * currentCollisionDistance;

        //플레이어 상태에 따른 카메라 Y 위치 조정, 앉을 때
        if (playerMovement != null && playerMovement.playerCrouch)
        {
            targetLocalPos.y = originalCamLocalY * 0.3f; 
        }
        else
        {
            //다시 원위치로
            targetLocalPos.y = originalCamLocalY; 
        }

        //부드럽게 목표 위치로 이동
        realCamera.localPosition = Vector3.Lerp(
            realCamera.localPosition,
            targetLocalPos,
            Time.deltaTime * smoothness
        );
    }

    //==> 카메라 흔들기 코루틴, 외부에서 데미지 받을 떄 호출하는 부분 <==
    public IEnumerator Shake(float duration, float magnitude)
    {
        if (realCam == null) yield break;

        Vector3 originalPos = realCam.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            realCam.transform.localPosition = new Vector3(
                originalPos.x + offsetX,
                originalPos.y + offsetY,
                originalPos.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        //원래 위치 복구
        realCam.transform.localPosition = originalPos;
    }
}