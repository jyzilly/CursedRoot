//using System.Collections;
//using UnityEngine;

//public class SpiderEgg : MonoBehaviour
//{
//    private bool isCounting = false;
//    private bool isPlayerInRange = false;
//    private Coroutine countdownCoroutine;
//    private EJPlayer currentPlayer;
//    private Rigidbody rb;
//    [SerializeField] GameObject fireSpider;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            Debug.Log($"{gameObject.name} �Ź̾��� �÷��̾ ����!");

//            currentPlayer = other.GetComponent<EJPlayer>();
//            if (currentPlayer != null)
//            {
//                currentPlayer.SetEscapeCallback(OnPlayerEscape); // X ��ư Ż�� ��� Ȱ��ȭ
//                currentPlayer.SetEggOneCallback(EggOne); // ù ��° X �Է� �� EggOne() ȣ��
//                currentPlayer.ResetXPressCount(); // X �Է� Ƚ�� �ʱ�ȭ
//            }

//            isPlayerInRange = true;

//            if (!isCounting)
//            {
//                isCounting = true;
//                countdownCoroutine = StartCoroutine(Countdown());
//            }
//        }
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player") && currentPlayer != null)
//        {
//            Debug.Log($"{gameObject.name} �÷��̾ ������ ���! X ��ư�� ��Ȱ��ȭ�˴ϴ�.");
//            currentPlayer.SetEscapeCallback(null);
//            currentPlayer.SetEggOneCallback(null); // ù ��° X �Է� ��ɵ� ��Ȱ��ȭ
//            currentPlayer = null;
//            isPlayerInRange = false;
//        }
//    }

//    IEnumerator Countdown()
//    {
//        float timer = 2f;
//        while (timer > 0)
//        {
//            Debug.Log($"{gameObject.name} ���� �ð�: {timer:F1}��");
//            timer -= 1f;
//            yield return new WaitForSeconds(1f);
//        }

//        Debug.Log($"{gameObject.name} �ð� �ʰ�! �÷��̾ �����߽��ϴ�.");
//        fireSpider.SetActive(true);
//        isCounting = false;
//    }

//    private void OnPlayerEscape()
//    {
//        Debug.Log($"{gameObject.name} �÷��̾ Ż���� �����߽��ϴ�!");

//        if (countdownCoroutine != null)
//        {
//            StopCoroutine(countdownCoroutine);
//            countdownCoroutine = null;
//        }

//        Destroy(gameObject);
//    }

//    private void EggOne()
//    {
//        Debug.Log("EggOne() �����!");

//        Transform meeple = this.gameObject.transform.GetChild(1);

//        foreach (Transform grandChild in meeple)
//        {
//            GameObject grandChildObject = grandChild.gameObject;
//            Debug.Log("���� ������Ʈ: " + grandChildObject.name);

//            // Rigidbody ��������
//            Rigidbody rb = grandChildObject.GetComponent<Rigidbody>();
//            if (rb != null)
//            {
//                // ���� isKinematic �� ���
//                Debug.Log($"{grandChildObject.name}�� isKinematic ����: {rb.isKinematic}");

//                // isKinematic �� ���� (��: false�� �����Ͽ� ���� ����)
//                rb.isKinematic = false;
//                Debug.Log($"{grandChildObject.name}�� isKinematic�� false�� ����!");
//            }
//            else
//            {
//                Debug.Log($"{grandChildObject.name}���� Rigidbody�� ����!");
//            }
//        }
//    }
//}
