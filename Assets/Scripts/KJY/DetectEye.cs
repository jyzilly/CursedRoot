using UnityEngine;


//나레이션 나오게 하는 부분, 콜레이더 감지될 때 나레이션 나오게 하는 것

public class DetectEye : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Collider eyeDetect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            dialogueManager.PlayDialogue("inPrison1", "player");
            eyeDetect.enabled = false; 
        }
    }

}
