using UnityEngine;
using UnityEngine.UI;

public class MentalGaugeUI : MonoBehaviour
{
    public Image mentalImage; // UI �̹���
    public PlayerMental playerMental; // ���ŷ� ���� Ŭ���� ����

    private float maxMental = 100f;

    void Update()
    {
        float cM = playerMental.currentMental;
        float fillValue = cM / maxMental;
        mentalImage.fillAmount = fillValue;
    }
}
