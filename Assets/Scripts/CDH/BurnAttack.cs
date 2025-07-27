using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BurnAttack : MonoBehaviour
{
    private Coroutine damageCoroutine;
    //[SerializeField] Player playrScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damageCoroutine = StartCoroutine(DealDamageOverTime(other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    IEnumerator DealDamageOverTime(Collider player)
    {
        Player players = player.GetComponent<Player>();
        while (true)
        {
            //player.GetComponent<Player>().TakeDamage(5); // ���� �Լ� ȣ��
            //player.GetComponent<Player>().StartCoroutine(PlayerHitEffect()); // ���� �Լ� ȣ��
            if (players != null)
            {
                players.TakeDamage(5);
                players.StartCoroutine(players.PlayerHitEffect());
            }

            yield return new WaitForSeconds(1f); // �ʴ� 1ȸ ����
        }
    }

}
