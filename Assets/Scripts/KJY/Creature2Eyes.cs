using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//미니프로젝트할 때 플레이어 외부를 검사할 때 사용하는 레이설정 가져와서 조금 고치기

[System.Serializable]
public struct CastInfo
{
    public bool Hit;
    public Vector3 Point;
    public float Distance;
    public float Angle;
}

public class Creature2Eyes : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private Creature2Manager creature2Manager;

    [Header("Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField, Range(0f, 30f)] private float viewRange = 10f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 40f;

    [Header("DebugSettings")]
    [SerializeField] private List<CastInfo> lineList;

    private WaitForSeconds checkDelay = new WaitForSeconds(0.1f);
    private Coroutine checkTargetCoroutine;
    private Coroutine drawRayLineCoroutine;


    void Start()
    {
        //Debug.Log("현재 설정된 playerLayer 값: " + playerLayer.value.ToString());
        lineList = new List<CastInfo>();
        StartCheckingTarget();
        //StartDrawingRayLines();
    }

    private void StartCheckingTarget()
    {
        if (checkTargetCoroutine == null)
        {
            checkTargetCoroutine = StartCoroutine(CheckTargetRoutine());
        }
    }

    //외부에서 중지 시키고 싶을 때 활성화 시키면 됨
    //public void StopCheckingTarget()
    //{
    //    if (checkTargetCoroutine != null)
    //    {
    //        StopCoroutine(checkTargetCoroutine);
    //        checkTargetCoroutine = null;
    //    }
    //}

    private IEnumerator CheckTargetRoutine()
    {
        while (true)
        {
            CheckTarget();
            yield return checkDelay;
        }
    }

    //부채꼴 시야
    private void CheckTarget()
    {
        Vector3 baseDir = transform.forward;
        //1도 레이 1개
        int rayCount = Mathf.RoundToInt(viewAngle);
        float halfAngle = viewAngle * 0.5f;
        float distance = viewRange;

        for (int i = 0; i < rayCount; i++)
        {
            //현재 순번에 해당하는 각도를 계산
            float angle = -halfAngle + (viewAngle * i / rayCount);
            //계산된 각도만큼 회전된 방향 벡터를 계산
            Vector3 dir = Quaternion.Euler(0, angle, 0) * baseDir;

            if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, distance, playerLayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    //Debug.Log("플레이어 감지됨!");
                    //creature2Manager.CommandTeleport();
                    creature2Manager.OnEyeDetected(transform.position);
                    break; //1회 감지만 하고 빠짐
                }
            }
        }
    }


    //디버그용
    private void StartDrawingRayLines()
    {
        if (drawRayLineCoroutine == null)
        {
            drawRayLineCoroutine = StartCoroutine(DrawRayLineRoutine());
        }
    }

    private void StopDrawingRayLines()
    {
        if (drawRayLineCoroutine != null)
        {
            StopCoroutine(drawRayLineCoroutine);
            drawRayLineCoroutine = null;
        }
    }

    private IEnumerator DrawRayLineRoutine()
    {
        while (true)
        {
            DrawRayLine();
            yield return null;
        }
    }

    private void DrawRayLine()
    {
        lineList.Clear();

        Vector3 baseDir = transform.forward;
        int rayCount = Mathf.RoundToInt(viewAngle);
        float halfAngle = viewAngle * 0.5f;
        float distance = viewRange;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = -halfAngle + (viewAngle * i / rayCount);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * baseDir;
            Vector3 origin = transform.position + Vector3.up;

            Ray ray = new Ray(origin, dir);
            CastInfo info = new CastInfo { Angle = angle };

            if (Physics.Raycast(ray, out RaycastHit hit, distance))
            {
                info.Hit = true;
                info.Point = hit.point;
                info.Distance = hit.distance;
                Debug.DrawLine(origin, hit.point, Color.red); // 감지 시 빨간선
            }
            else
            {
                info.Hit = false;
                info.Point = origin + dir * distance;
                info.Distance = distance;
                Debug.DrawLine(origin, origin + dir * distance, Color.green); // 미감지시 초록선
            }

            lineList.Add(info);
        }
    }
}
