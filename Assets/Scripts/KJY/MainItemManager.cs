using UnityEngine;

public class MainItemManager : MonoBehaviour
{
    public static MainItemManager Instance { get; private set; }


    public int mainItem = 0;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            //씬이 전환되어도 이 게임 오브젝트가 파괴되지 않도록 설정
            DontDestroyOnLoad(gameObject);
            //게임 시작 시 저장된 메인아이템 수 불러오기, 
            mainItem = PlayerPrefs.GetInt("MainItemCount", 0);
        }
        else
        {
            //새로 생긴 것을 파괴하여 단 하나만 존재하도록 보장, 충돌방지
            Destroy(gameObject);
        }
        Debug.Log(mainItem);
    }

}
