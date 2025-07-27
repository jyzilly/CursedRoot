using UnityEngine;


//눈알에 많이 감지되면 호출하는 코드, 1,2,3 회수 따라 벽이 올라는 수량 다르게 설정, 3번이면 전체 벽이 올라간다..

public class WallPosController : MonoBehaviour
{

    [SerializeField] private GameObject[] wall;
    [SerializeField] private GameObject[] fixedwall;

    [SerializeField] private float moveDistance = 13f;

    private Vector3 tarfetPos;


    public void MoveThewall(int Cnt)
    {
        if(Cnt == 1)
        {
            //Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.Pwall);
            wallPos();
        }
        else if (Cnt == 2)
        {
            //Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.Pwall);
            otherWallPos();
        }
        else if (Cnt == 3)
        {
            //Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.Pwall);
            fixedWallPos();
        }
    }

    private void wallPos( )
    {
        for (int i = 0; i < 2; ++i )
        {
            Move(i);
        }
    }

    private void otherWallPos()
    {
        for (int i = 2; i < wall.Length; ++i)
        {
            Move(i);
        }
    }

    private void fixedWallPos()
    {
        for(int i = 0; i < fixedwall.Length; ++i)
        {
            Move(i);
        }
    }

    private void Move(int a)
    {
        tarfetPos = wall[a].transform.position;
        tarfetPos.y = moveDistance;
        wall[a].transform.position = tarfetPos;
    }
}
