using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BrokenWall : MonoBehaviour
{
    [Header("�� ���� ����")]
    [SerializeField] private GameObject wholeWall;
    [SerializeField] private GameObject wallPieceParent;

    [Header("���߷� ����")]
    [SerializeField] private float explosionForce = 200f;
    [SerializeField] private float explosionRadius = 2f;

    [Header("�ܰ� ��")]
    [SerializeField] private int totalStages = 5;
    private int pressCount = 0;

    //[Header("UI")]
    //[SerializeField] private Slider breakProgressSlider;
    //[SerializeField] private GameObject breakSliderObject;

    //[Header("���� �� ��� ������")]
    //[SerializeField] private GameObject dropItemPrefab;

    [Header("���� �� ���̾� �ٲ� ������Ʈ")]
    [SerializeField] private GameObject brokenWall;

    private List<Transform> fragments = new List<Transform>();
    private int piecesPerStage;

    [SerializeField] private Collider wall;
    [SerializeField] private GameManager GM;


    private void Start()
    {
        if (wallPieceParent != null)
        {
            wallPieceParent.SetActive(false);
            foreach (Transform frag in wallPieceParent.transform)
            {
                fragments.Add(frag);
                var rb = frag.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
        }

        piecesPerStage = Mathf.CeilToInt(fragments.Count / (float)totalStages);

        //if (breakProgressSlider != null)
        //{
        //    breakProgressSlider.minValue = 0;
        //    breakProgressSlider.maxValue = totalStages;
        //    breakProgressSlider.value = 0;
        //}

        //if (breakSliderObject != null)
        //    breakSliderObject.SetActive(false);
    }

    public void BreakOnce()  // EŰ�� ȣ��
    {
        //if (pressCount >= totalStages) return;

        pressCount++;

        if (pressCount == 1)
        {
            if (wholeWall != null) wholeWall.SetActive(false);
            if (wallPieceParent != null) wallPieceParent.SetActive(true);
            //if (breakSliderObject != null) breakSliderObject.SetActive(true);
        }

        ApplyBreakStage(pressCount);
    }

    private void ApplyBreakStage(int stage)
    {
        int start = (stage - 1) * piecesPerStage;
        int end = Mathf.Min(stage * piecesPerStage, fragments.Count);

        for (int i = start; i < end; i++)
        {
            var frag = fragments[i];
            var rb = frag.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                //if (rb.gameObject.CompareTag("Player")) return; // �÷��̾�� ����

                rb.AddExplosionForce(explosionForce, wallPieceParent.transform.position, explosionRadius);
                Destroy(frag.gameObject, 1.5f);

            }
        }

        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.wall1);
        GM.EmitSound(transform.position);

        //if (breakProgressSlider != null)
        //    breakProgressSlider.value = pressCount;

        if (pressCount >= totalStages)
        {
            //if (dropItemPrefab != null)
            //    Instantiate(dropItemPrefab, transform.position + Vector3.up, Quaternion.identity);

            //if (breakSliderObject != null)
            //    breakSliderObject.SetActive(false);
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.wallBroken);


            if (brokenWall != null)
                brokenWall.layer = LayerMask.NameToLayer("Default"); // ����\

            wall.enabled = false;
        }
    }
}
