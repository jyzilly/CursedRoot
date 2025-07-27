using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LightProp : MonoBehaviour
{
    [Tooltip("0-based unique lamp ID")]
    public int lampID;

    public GameObject icon;     // EŰ UI ������
    public GameObject lightObj; // ���� ���� ������Ʈ
    public Renderer lantern;    // Emission �� �����
    public int uvIndex = 0;     // Emission ��Ƽ���� �ε���

    [Header("Sound")]
    public AudioClip lightOnClip;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;

    [Header("Detection")]
    public float detectRange = 3.0f;  // �÷��̾���� �ִ� ��ȣ�ۿ� �Ÿ�

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.0f;  // 2D ����
    }

    /// <summary>
    /// EŰ�� ������ �� �� ������ ���� ���� �ȿ� �ִ���
    /// </summary>
    public bool IsInRange(Transform player)
    {
        return Vector3.Distance(transform.position, player.position) <= detectRange;
    }

    /// <summary>
    /// UI ������ on/off
    /// </summary>
    public void ShowIcon(bool show)
    {
        if (icon) icon.SetActive(show);
    }

    /// <summary>
    /// ���� �Ѱ� ���� + �Ҹ�
    /// </summary>
    public void SetState(bool on)
    {
        if (lightObj) lightObj.SetActive(on);

        if (lantern)
            lantern.materials[uvIndex]
                   .SetColor("_EmissionColor", on ? Color.white : Color.black);

        if (on && lightOnClip)
            audioSource.PlayOneShot(lightOnClip, volume);
    }
}
