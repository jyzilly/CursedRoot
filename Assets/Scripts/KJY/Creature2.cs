using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Audio;

//상태를 만들어서 한번에 상태 하나만 실행하게 하고 서로 꼬이지 않게, 대충 : 순찰, 둘러보기, 추적, 공격

public class Creature2 : MonoBehaviour
{
    [Header("Defalut Setting")]
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private Player player;
    [SerializeField] private CameraMovement camShake;
    [SerializeField] private AudioClip shutClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip walkClip;

    [Header("AI Settings")]
    [SerializeField] private Transform[] wayPoint;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float pursuitSpeed = 10.0f;

    [Header("Creature2 Stats")]
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float detectionRange = 10f;
    public float damage = 15f;

    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 2.0f;

    private enum CreatureState { Patrol, Pursuit, Attack, Idle };
    private CreatureState currentState;

    private bool isAttacking;
    private int currentPatrolIndex = 0;
    private bool patrollingForward = true;
    private float idleTimer = 0f;
    private bool isReversingCooldown = false;
    private bool justExitedIdle = false;

    //크리처 애니메이션
    private Animator animator;
    private AudioSource audioSource;

    //flashTimer 활성화 여부
    public bool flashOn;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.Stop();
        isAttacking = false;
        currentState = CreatureState.Patrol;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (isAttacking)
        {
            //만약에 공격중이면 건너뛰기
        }
        //후레쉬 켜진 상태
        else if (flashOn)
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = CreatureState.Attack;
            }
            else
            {
                currentState = CreatureState.Pursuit;
            }
        }
        else
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = CreatureState.Attack;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = CreatureState.Pursuit;
            }
            else
            {
                if (currentState == CreatureState.Pursuit || (currentState != CreatureState.Idle && currentState != CreatureState.Patrol))
                {
                    currentState = CreatureState.Patrol;
                }
            }
        }

        switch (currentState)
        {
            case CreatureState.Patrol:
                Creature2Patrol();
                break;
            case CreatureState.Attack:
                Creature2Attack();
                break;
            case CreatureState.Idle:
                Creature2Idle();
                break;
            case CreatureState.Pursuit:
                Creature2Pursuit();
                break;
        }



    }

    private void Creature2Patrol()
    {
        //루틴대로 걸어가지만 만약에 앞에 막혀있으면 반대로 첫번째 지점으로 돌아가기

        navMeshAgent.speed = patrolSpeed;

        if (wayPoint.Length == 0)
        {
            if (currentState != CreatureState.Idle) currentState = CreatureState.Idle;
            return;
        }

        //장애물이 있는지 확인
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f; 
        //바라보는 방향에 쏘기
        Vector3 direction = transform.forward; 

        //isReversingCooldown -> 무한 반복 방지
        if (!isReversingCooldown && Physics.Raycast(raycastOrigin, direction, out hit, raycastDistance))
        {
            //벽이라면
            if (hit.collider.CompareTag("Wall"))
            {
                //Debug.Log("전방에 'Wall' 장애물 감지! 순찰 방향을 반대로 전환합니다.");
                //방향 전환 함수 호출
                ReversePatrolDirection(); 
                return;
            }
        }

        //레이 시각적으로 확인
        //Debug.DrawRay(raycastOrigin, direction * raycastDistance, Color.red);



        navMeshAgent.isStopped = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);
        animator.SetBool("isRun", false);

        if (audioSource.clip != walkClip || !audioSource.isPlaying)
        {
            audioSource.clip = walkClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        //wayPoint 없을 경우
        if (currentPatrolIndex < 0 || currentPatrolIndex >= wayPoint.Length || wayPoint[currentPatrolIndex] == null)
        {
            Debug.LogWarning("Creature2: Invalid waypoint or index. Resetting patrol index.");
            currentPatrolIndex = 0;
            if (wayPoint.Length == 0 || wayPoint[currentPatrolIndex] == null)
            {
                if (currentState != CreatureState.Idle) currentState = CreatureState.Idle;
                return;
            }
        }


        navMeshAgent.destination = wayPoint[currentPatrolIndex].position;

        //순찰 지점에 도착했을 때 잠시 Idle상태로 전한 돌아보는 느낌으로
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
            {
                currentState = CreatureState.Idle;
                idleTimer = 0f;
            }
        }

        if (!justExitedIdle)
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                {
                    currentState = CreatureState.Idle;
                    idleTimer = 0f;
                }
            }
        }

        //다음 프레임부터는 정상적으로 도착 검사를 하도록 플래그를 리셋
        justExitedIdle = false;
    }

    private void ReversePatrolDirection()
    {
        //방향을 바꿔서
        patrollingForward = !patrollingForward;
        //다음 웨이포인트를 계산
        TheNextWayPoint();
        //짧은 쿨타임을 시작하여 무한정 방향을 바꾸는 것을 방지
        StartCoroutine(ReverseCooldown());
    }

    //1초 동안은 다시 방향  바꾸는 거 금지
    private IEnumerator ReverseCooldown()
    {
        isReversingCooldown = true;
        yield return new WaitForSeconds(1.0f); 
        isReversingCooldown = false;
    }

    //다음 wayPoint로 
    private void TheNextWayPoint()
    {
        if (wayPoint.Length == 0) return;

        if (patrollingForward)
        {
            ++currentPatrolIndex;
            //돌아가기, 자신보다 작은 위치로
            if (currentPatrolIndex >= wayPoint.Length)
            {
                currentPatrolIndex = wayPoint.Length > 1 ? wayPoint.Length - 2 : 0;
                patrollingForward = false;
            }
        }
        else
        {
            --currentPatrolIndex;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = wayPoint.Length > 1 ? 1 : 0;
                patrollingForward = true;
            }
        }

        currentState = CreatureState.Patrol;
        justExitedIdle = true;
    }

    private void Creature2Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        navMeshAgent.isStopped = true;

        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", false);
        animator.SetTrigger("Attack");

        audioSource.Stop();
        audioSource.PlayOneShot(shutClip);

        //피효과
        if (player != null) player.StartCoroutine(player.PlayerHitEffect());

        //공격하면 현제 손에 있는 아이템을 떨어트리기
        if (itemmanager != null && itemmanager.currentItem != null && !itemmanager.currentItem.CompareTag("Flashlight"))
        {
            itemmanager.DropCurrentItem();
        }

        //공격하면 화면 카메라 흐들리는 효과 호출
        if (camShake != null) camShake.StartCoroutine(camShake.Shake(0.2f, 0.3f));

        if (player != null)
        {
            //플레이어 바라보는 방향 계산
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            //slerp 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * navMeshAgent.angularSpeed);
        }

        //다음 공격까지 쿨타임 가지기
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    //애니메이션이벤트에 호출
    public void EndAttack()
    {
        //상태 초기화
        isAttacking = false;
        navMeshAgent.isStopped = false;

        if (player == null)
        {
            currentState = CreatureState.Patrol;
            return;
        }

        //거리다시계산해서 상태 판단
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= attackRange)
        {
            if (player != null) player.TakeDamage(damage);
            //Debug.Log("Damage");
            //Creature2Attack();
        }

        //후레쉬 5초이상 켜지는 경우
        if (flashOn)
        {
            if (distanceToPlayer <= attackRange)
            {
                Creature2Attack();
            }
            else
            {
                currentState = CreatureState.Pursuit;
            }
        }
        //아닌 경우
        else
        {
            if (distanceToPlayer <= attackRange)
            {
                Creature2Attack();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = CreatureState.Pursuit;
            }
            else
            {
                currentState = CreatureState.Patrol;
            }
        }

        //상태 결정하고 상태를 업데이튼
        UpdateAnimatorForState(currentState);
    }

    private void UpdateAnimatorForState(CreatureState targetState)
    {
        animator.SetBool("isIdle", targetState == CreatureState.Idle);
        animator.SetBool("isWalking", targetState == CreatureState.Patrol);
        animator.SetBool("isRun", targetState == CreatureState.Pursuit);
    }

    //플레이어 위치로 추적
    private void Creature2Pursuit()
    {
        navMeshAgent.speed = pursuitSpeed;

        if (player == null)
        {
            currentState = CreatureState.Patrol;
            return;
        }

        navMeshAgent.isStopped = false;
        navMeshAgent.destination = player.transform.position;

        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", true);

        if (audioSource.clip != runClip || !audioSource.isPlaying)
        {
            audioSource.clip = runClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Creature2Idle()
    {
        navMeshAgent.isStopped = true;

        animator.SetBool("isIdle", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", false);

        if (audioSource.isPlaying && (audioSource.clip == walkClip || audioSource.clip == runClip))
        {
            audioSource.Stop();
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0f;
            TheNextWayPoint();
        }

    }

}


