using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Spider2 : MonoBehaviour
{
    [Header("��������Ʈ��")]
    public Transform[] waypoints;

    [Header("Ž��/���� �Ÿ�")]
    public float detectionRadius = 10f;
    public float attackRange = 2f;

    [Header("���� ��ٿ�(��)")]
    public float attackCooldown = 1.5f;

    [Header("���� �ִ� ����(��)")]
    [Tooltip("Animator���� Attack Ŭ�� ��� ���̿� ���� �ּ���.")]
    public float attackAnimDuration = 0.8f;

    private NavMeshAgent _agent;
    private Animator _anim;
    private Transform _player;

    private int _currentWaypoint = 0;
    private float _nextAttackTime = 0f;
    private bool _isAttacking = false;

    private enum State { Patrol, Chase, Attack }
    private State _state = State.Patrol;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
        if (_anim == null)
            Debug.LogError($"{name}: �ڽĿ� Animator�� �����ϴ�.");

        var go = GameObject.FindWithTag("Player");
        if (go != null) _player = go.transform;
        else Debug.LogError($"{name}: Player �±װ� �����ϴ�.");
    }

    void Start()
    {
        GoToNextWaypoint();
    }

    void Update()
    {
        if (_player == null) return;

        float d = Vector3.Distance(transform.position, _player.position);

        // Patrol �� Chase ��ȯ
        if (d <= detectionRadius && _state == State.Patrol)
            _state = State.Chase;
        else if (d > detectionRadius && _state != State.Patrol)
        {
            _state = State.Patrol;
            GoToNextWaypoint();
        }

        // ���º� ó��
        switch (_state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chase: ChaseUpdate(d); break;
            case State.Attack: AttackUpdate(d); break;
        }
    }

    void PatrolUpdate()
    {
        // �ڵ� ����
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
            GoToNextWaypoint();
        }

        _anim?.SetBool("isWalk", true);
        _anim?.SetBool("isAttack", false);
    }

    void ChaseUpdate(float d)
    {
        _agent.SetDestination(_player.position);
        _anim?.SetBool("isWalk", true);
        _anim?.SetBool("isAttack", false);

        if (d <= attackRange)
            _state = State.Attack;
    }

    void AttackUpdate(float d)
    {
        // ���߱�
        _agent.ResetPath();
        _anim?.SetBool("isWalk", false);

        // ��Ÿ� ���̰�, ���� ��ٿ� ������, �ڷ�ƾ ���� �ƴϸ� ���� ����
        if (d <= attackRange && !_isAttacking && Time.time >= _nextAttackTime)
        {
            StartCoroutine(DoAttack());
        }
        // ��Ÿ� ����� �ٽ� Chase
        else if (d > attackRange)
        {
            _state = State.Chase;
        }
    }

    IEnumerator DoAttack()
    {
        _isAttacking = true;
        _anim?.SetBool("isAttack", true);

        // �ִϸ��̼� ���� �ð� ��ŭ ���
        yield return new WaitForSeconds(attackAnimDuration);

        // ���� ������
        _anim?.SetBool("isAttack", false);
        _isAttacking = false;
        _nextAttackTime = Time.time + attackCooldown;
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_currentWaypoint].position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
