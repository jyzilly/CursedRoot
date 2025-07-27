using UnityEngine;
using System.Collections;


//크리처2하고 유사하게 만들면 됨, 눈알하고 플레이어정면 함수 따로 만들어서 호출, 호출위치하고 정면 이런 설정 조금 달라서

public class Creature2Clone : MonoBehaviour
{

    [Header("Scripts Settings")]
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private Player player;
    [SerializeField] private CameraMovement camShake;

    [Header("Default Settings")]
    [SerializeField] private int damege;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private AudioClip shutClip;

    private Transform playerTransform;
    private Animator animator;
    private bool isAttacking = false;
    private AudioSource audioSource;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    //눈알 소환될 때 사용하는 함수
    public void InitializeAtPosition(Vector3 eyePos, Transform player)
    {
        playerTransform = player;

        //Y축 높이만 플레이어와 동일
        Vector3 finalPosition = eyePos;
        finalPosition.y = player.position.y;
        transform.position = finalPosition;

        //생성되자마자 플레이어를 바라보도록 방향 설정
        LookAtPlayer();
    }

    //플레이어 정면에 소환될 때 사용하는 함수
    public void InitializeInFrontOfPlayer(Transform player)
    {
        playerTransform = player;

        //플레이어의 위치와 바라보는 방향을 기준으로 스폰 위치 계산
        Vector3 spawnPosition = playerTransform.position + playerTransform.forward * spawnDistance;
        spawnPosition.y = playerTransform.position.y;
        transform.position = spawnPosition;

        //생성되자마자 플레이어를 바라보도록 방향 설정
        LookAtPlayer();
    }

    //공통 기능 -> 플레이어를 즉시 바라보게 하는 함수
    private void LookAtPlayer()
    {
        if (playerTransform == null) return;
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist > attackRange)
        {
            Chase();
        }
        else
        {
            //공격
            if (!isAttacking)
            {
                Attack();
            }

        }
    }

    private void Chase()
    {
        //추적
        Vector3 dir = (playerTransform.position - transform.position);
        dir.y = 0;
        dir.Normalize();

        //회전
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

        //이동
        transform.position += dir * moveSpeed * Time.deltaTime;

        //애니메이션
        animator.SetBool("run", true);
        animator.ResetTrigger("attack");
        isAttacking = false;
    }

    private void Attack()
    {
        isAttacking = true;
        animator.SetBool("run", false);
        animator.SetTrigger("attack");
        audioSource.Stop();
        audioSource.PlayOneShot(shutClip);
    }

    public void ApplyAttackDamage()
    {
        if (player == null) return;

        player.TakeDamage(damege);
        //Debug.Log("때리는중");
        player.StartCoroutine(player.PlayerHitEffect());

        //손에 아이템 있으면 호출
        if (itemmanager.currentItem != null && !itemmanager.currentItem.CompareTag("Flashlight"))
        {
            itemmanager.DropCurrentItem();
        }

        camShake.StartCoroutine(camShake.Shake(0.2f, 0.3f));
    }

    public void EndAttack()
    {
        isAttacking = false;
    }
}
