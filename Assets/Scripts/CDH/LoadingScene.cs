using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    public Slider progressBar; // Inspector���� Slider ����

    private float targetProgress = 0;

    void Start()
    {
        StartCoroutine(DelayThenLoad());
    }

    IEnumerator DelayThenLoad()
    {
        yield return null; // �� ������ ���

        StartCoroutine(LoadNextScene());
    }


    IEnumerator LoadNextScene()
    {
        string nextScene = PlayerPrefs.GetString("NextScene"); // ����� �� �̸� ��������

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene); // �񵿱� �ε� ����
        op.allowSceneActivation = false; // �ε� �Ϸ� �� �ڵ� ��ȯ X

        while (!op.isDone)
        {
            // ���൵�� 0~1 ���̷� ���
            float rawProgress = Mathf.Clamp01(op.progress / 0.9f);
            targetProgress = rawProgress;

            progressBar.value = Mathf.Lerp(progressBar.value, targetProgress, Time.deltaTime * 2.5f);


            if (op.progress >= 0.9f && progressBar.value >= 0.98f)
            {
                progressBar.value = 1f;
                // ��� �����̸� �ְų� �ε� �Ϸ� UI ǥ�� ����
                yield return new WaitForSeconds(0.5f);
                op.allowSceneActivation = true; // ���� �� ��ȯ ���
            }

            yield return null;
        }
    }
}
