using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
    // �ٲ� Skybox ��Ƽ����
    public Material targetSkybox;

    // �ñ׳ο� ������ �޼���
    public void ChangeSkybox()
    {
        if (targetSkybox != null)
            RenderSettings.skybox = targetSkybox;
        DynamicGI.UpdateEnvironment();  // GI ����
    }
}
