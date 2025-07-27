using UnityEngine;
using UnityEngine.UI;

public class FlashManager : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private Light flashlightLight;

    [Header("Camera Tracking")]
    [SerializeField] private Transform cameraTransform;

    [Header("Battery Settings")]
    //배터리 기본 전량
    [SerializeField] private float maxBattery = 1000f;
    [SerializeField] private float currentBattery = 1000f;
    //초당 배터리 감소량
    [SerializeField] private float batteryDrainRate = 5f;

    [Header("Battery UI")]
    [SerializeField] private Image batteryUI;

    [Header("Flash UI Settings")]
    [SerializeField] private GameObject flashUI;

    [Header("Flash Particle")]
    public ParticleSystem flashUIParticleSystem;

    [Header("Creature2 Settings")]
    [SerializeField] private Creature2 creature2;
    //직접 조준 감지 거리
    [SerializeField] private float flashlightRange = 30f;
    //직접 조준 감지 각도
    [SerializeField] private float flashlightAngle = 30f; 

    private bool isOn = false;
    private bool isHeld = false;
    private float flashOnDurationTimer = 0f;


    private void Awake()
    {
        if (flashlightLight == null) flashlightLight = GetComponentInChildren<Light>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (flashUI != null) flashUI.SetActive(false);
        if (flashlightLight != null) flashlightLight.enabled = false;
        UpdateBatteryUI();
    }

    private void Update()
    {
        if (isHeld && isOn)
        {
            flashUI.SetActive(true);
            DrainBattery();

            //후레쉬 timer
            flashOnDurationTimer += Time.deltaTime;
            if (flashOnDurationTimer >= 5f)
            {
                //5초가 지나면 무조건 추적
                if (creature2 != null) creature2.flashOn = true;
            }

            //직접 비추면 즉시 추격
            CheckIfLightHitsCreatureDirectly();

        }
        else
        {
            flashUI.SetActive(false);
            //손전등이 꺼지면 모든 관련 상태를 리셋
            ResetAllFlashStates();
        }
    }

    //빛이 크리처에 직접 닿았는지 확인하고, 닿았다면 즉시 추적
    private void CheckIfLightHitsCreatureDirectly()
    {
        if (creature2 == null) return;

        //이미 flashOn이 활성화 되었다면, 5초이상 켜진상태라는 뜻으로 체크할 필요없음
        if (creature2.flashOn) return;

        Vector3 directionToCreature = creature2.transform.position - cameraTransform.position;
        //Debug.DrawRay(cameraTransform.position, cameraTransform.forward *flashlightRange, Color.red);


        if (directionToCreature.magnitude < flashlightRange && Vector3.Angle(cameraTransform.forward, directionToCreature) < flashlightAngle / 2)
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, directionToCreature.normalized, out hit, flashlightRange))
            {
                if (hit.collider.gameObject == creature2.gameObject)
                {
                    //Debug.DrawRay(cameraTransform.position, directionToCreature.normalized * hit.distance, Color.red);

                    //직접비추고 있다는 뜻 true로
                    creature2.flashOn = true;
                }
            }
        }
    }

    //상태 리셋
    private void ResetAllFlashStates()
    {
        flashOnDurationTimer = 0f;
        if (creature2 != null && creature2.flashOn)
        {
            creature2.flashOn = false;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform != null)
        {
            transform.rotation = cameraTransform.rotation;
        }
    }

    public void Toggle()
    {
        if (currentBattery <= 0f && !isOn)
        {
            //Debug.Log("배터리 없음");
            return;
        }

        isOn = !isOn;
        if (flashlightLight != null) flashlightLight.enabled = isOn;
        //Debug.Log("Flashlight isOn: " + isOn);

        //껴지면 모든 상태 리셋
        if (!isOn)
        {
            ResetAllFlashStates();
        }
    }

    //외부 컨트럴할 때
    public bool IsOn() => isOn;

    //itemManager에서 제어할 수 있게
    public void TurnOn()
    {
        if (currentBattery > 0f)
        {
            isOn = true;
            if (flashlightLight != null) flashlightLight.enabled = true;
        }
    }

    public void TurnOff()
    {
        isOn = false;
        if (flashlightLight != null) flashlightLight.enabled = false;

        ResetAllFlashStates();
    }

    //itemManager에서 가지고 있으면 여기상태도 같이 바꿀 수 있게 
    public void SetHeld(bool held)
    {
        isHeld = held;
        if (!held)
        {
            ResetAllFlashStates();
        }
    }

    private void DrainBattery()
    {
        currentBattery -= batteryDrainRate * Time.deltaTime;

        if (currentBattery <= 0f)
        {
            currentBattery = 0f;
            TurnOff();
            //Debug.Log("배터리 소진");
        }

        UpdateBatteryUI();
    }

    //itemManager에서 베터리를 직업하면 전력 채울 수 있게
    public void RefillBattery(float amount)
    {
        currentBattery = Mathf.Clamp(currentBattery + amount, 0, maxBattery);
        //Debug.Log("배터리 충전됨: " + currentBattery);

        UpdateBatteryUI();
    }

    //잔여량 표시
    private void UpdateBatteryUI()
    {
        if (batteryUI != null)
        {
            batteryUI.fillAmount = currentBattery / maxBattery;
        }
    }

}
