using UnityEngine;
using System;

public class Statue : MonoBehaviour
{
    public bool isLit = false;                    // ���� ��������
    public event Action<Statue> OnFireLit;        // �� �� �� ȣ���� �̺�Ʈ
    //public GameObject fireEffect;             // �� �� �� ��Ÿ�� ��ƼŬ
    public ParticleSystem fireEffect;
    [SerializeField] AudioSource fireAudio;

    private void Awake()
    {
        fireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void LightFire()
    {
        if (isLit) return;
        isLit = true;

        //fireEffect.SetActive(true);
        fireEffect.Play(true); // ��ƼŬ ���


        // ����Ʈ, �Ҹ� �� �߰� ����
        Debug.Log($"{gameObject.name} �� ����");
        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.fireOn);
        fireAudio.Play();

        OnFireLit?.Invoke(this);                 // ���� �����ڿ��� �˸�
    }

    public void Extinguish()
    {
        isLit = false;
        // �� ���� ����Ʈ/���� �ֱ�
        if (fireEffect != null)
        {
            //fireEffect.SetActive(false);
            fireEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireAudio.Stop();
        }
    }
}
