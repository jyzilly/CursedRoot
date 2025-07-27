using System.Collections;
using TMPro;
using UnityEngine;

public class Egg : MonoBehaviour
{
    private float countdownTime = 3f; // ī��Ʈ�ٿ� �ð�
    //public TextMeshProUGUI countdownText;
    private bool isCountingDown = false; // ī��Ʈ�ٿ� ������ Ȯ��
    [SerializeField] GameObject effect;
    [SerializeField] GameObject cpSpider;
    

    //private void OnCollisionEnter(Collision col)
    //{
    //    if (col.gameObject.CompareTag("Player") && !isCountingDown)
    //    {
    //        StartCoroutine(StartCountdown());
    //    }
    //}

    private void Start()
    {
        //egg = gameObject.GetComponent<GameObject>();
    }

    private void OnTriggerEnter(Collider juju)
    {
        if (juju.gameObject.CompareTag("Player") && !isCountingDown)
        {
            StartCoroutine(StartCountdown());
        }
    }

    IEnumerator StartCountdown()
    {
        isCountingDown = true;
        float timeLeft = countdownTime;

        while (timeLeft > 0)
        {
            Debug.Log(timeLeft.ToString("F0"));

            yield return new WaitForSeconds(1f);
            timeLeft--;
        } 
        if(timeLeft == 0)
        {
            StartCoroutine(EffectRoutine());
        }


        isCountingDown = false; // ī��Ʈ�ٿ� ���� �� �ٽ� ���� ����
    }

    IEnumerator EffectRoutine()
    {
        effect.SetActive(true);
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
        //Instantiate(cpSpider);
        cpSpider.SetActive(true);
        Debug.Log("153");
    }

   
}
