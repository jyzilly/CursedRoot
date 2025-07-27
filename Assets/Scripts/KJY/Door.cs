using UnityEngine;


//애니메이션 미리 설정해놓고, 두 가지 상태를 인식하면서 교환실행, 문이 많아서 오디오, 스크립트에서 직접실행하는 것이 더 효율

public class Door : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private AudioSource audioSource;
    private bool toggleState = false;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void TheDorrControl()
    {
        if (animator != null)
        {
            //토클상태로 오픈인지 크로즈인지 구분
            if (toggleState)
            {
                animator.Play("DoorClose");
                PlaySound(closeSound);
                toggleState = false;
            }
            else
            {
                animator.Play("DoorOpen");
                PlaySound(openSound);
                toggleState = true;
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

}
