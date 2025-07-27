using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    AudioSource bgmPlayer;

    [Header("SFX")]
    public AudioClip[] sfxClip;
    public float sfxVolume;
    public int channels;
    AudioSource[] sfxPlayers;
    int channelIndex;

    //�� ȿ������ �ν����� ������� �̸�����
    public enum sfx { NRwCry, NRlighter, NRwind, NRknock, NRhorror, NRthunderstorm, NRbigFire, PbabyCry, Pwall, Phorror, PwaterDrop, PlayerWalk, PlayerRun, PlayerBreathe, PlayerItem, PlayerwalkSlowly }

    // SFX �ߺ� ��� ����
    private Dictionary<sfx, float> sfxLastPlayTime = new Dictionary<sfx, float>();
    public float sfxCooldown = 0.1f;

    void Awake()
    {
        instance = this;
        Init();
    }

    void Init()
    {
        //
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;


        // SFX ä�� �ʱ�ȭ
        GameObject sfxObject = new GameObject("sfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];
        for (int index = 0; index < sfxPlayers.Length; ++index)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].loop = false;
        }

        // SFX ��Ÿ�� �ʱ�ȭ
        foreach (sfx s in System.Enum.GetValues(typeof(sfx)))
        {
            sfxLastPlayTime[s] = -999f;
        }
    }


    //����ϴ� ���
    //����� ���;��ϴ� ��������
    //AudioManager.instance.PlaySfx(AudioManager.Sfx.�̶� �̸� ������ �� enum ȿ���� �̸��� �Է�);
    public void PlaySfx(sfx sfx)
    {
        // ��Ÿ�� �˻�
        if (Time.time - sfxLastPlayTime[sfx] < sfxCooldown)
            return;

        bool played = false;

        for (int index = 0; index < sfxPlayers.Length; ++index)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;/*�� �������� �������ߴ� �÷��̾��� �ε���*/
            if (sfxPlayers[loopIndex].isPlaying)
            {
                channelIndex = loopIndex;
                sfxPlayers[loopIndex].clip = sfxClip[(int)sfx];
                sfxPlayers[loopIndex].Play();
                played = true;
                break;
            //���� �̸��� ȿ������ ������ ���� �� �� �������� �Ҹ��� ���  ��Ű�� ������
            //���� ������ �ִ°� ������ ������ ����ġ ������ �����ָ� �ȴ�.
            //int ranIndex = 0;
            //if(sfx == sfx.Hit || sfx == sfx.Melee)
            //{
            //    ranIndex = Random.Range(0, 2);
            //}
            }
        }

            // ��� ä���� ��� ���� ���, ���� ������ ä�� ���� ���� (ȿ���� ���� ����)
        if (!played)
        {
                channelIndex = (channelIndex + 1) % sfxPlayers.Length;
                sfxPlayers[channelIndex].clip = sfxClip[(int)sfx];
                sfxPlayers[channelIndex].Play();
        }

        sfxLastPlayTime[sfx] = Time.time;

        // channel �ϳ��� ����
        //sfxPlayers[0].clip = sfxClip[(int)sfx];
        //sfxPlayers[0].Play();
    }

    public void Play3DSfx(sfx sfx, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        // �ӽ� ����� �ҽ� ������Ʈ ����
        GameObject tempSfxObject = new GameObject("Temp3DSfx");
        tempSfxObject.transform.position = position;
        AudioSource tempAudio = tempSfxObject.AddComponent<AudioSource>();

        tempAudio.clip = sfxClip[(int)sfx];
        tempAudio.volume = volume;
        tempAudio.spatialBlend = 1f;             // 3D ȿ�������� ���� (0:2D, 1:3D)
        tempAudio.minDistance = minDistance;      // �ּ� �Ÿ�
        tempAudio.maxDistance = maxDistance;      // �ִ� �Ÿ�
        tempAudio.rolloffMode = AudioRolloffMode.Logarithmic; // ���� ���

        tempAudio.Play();

        // AudioClip ���� + �ణ�� ���� �Ŀ� �ӽ� ������Ʈ ����
        Destroy(tempSfxObject, tempAudio.clip.length + 0.5f);
    }
}