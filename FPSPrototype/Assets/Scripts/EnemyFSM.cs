using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState { None = -1, Idle = 0, Wander, Pursuit, Attack, }
public class EnemyFSM : MonoBehaviour
{
    [Header("Persuit")]
    [SerializeField]
    private float targetRecognitionRange = 30;      // 적이 플레이어를 인지하는 거리
    [SerializeField]
    private float pursuitLimitRange = 10;               // 거리가 10이상 멀어지면 배회상태로 전환

    [Header("Attack")]
    [SerializeField]
    private GameObject projectilePrefab;            // 총알 프리팹
    [SerializeField]
    private Transform projectileSpawnPoint;         // 총알 스폰포인트
    [SerializeField]
    private float attackRange = 30;                     // 적 공격 사거리
    [SerializeField]
    private float attackRate = 0.5f;                    // 적 공격속도

    [Header("Audioclips")]
    [SerializeField]
    private AudioClip audioclipfire;
    private AudioSource audioSource;

    private EnemyState enemyState = EnemyState.None;
    private float lastAttackTime = 0;

    private PlayerStatus status;                                // 이동속도
    private NavMeshAgent navMeshAgent;                  // 유니티 네비게이션을 이용한 적 이동제어
    private Transform target;                                   // 적의 공격 대상(플레이어)
    private EnemyMemoryPool enemyMemoryPool;

    //private void Awake()
    public void Setup(Transform target, EnemyMemoryPool enemyMemoryPool)
    {
        status = GetComponent<PlayerStatus>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        this.target = target;
        this.enemyMemoryPool = enemyMemoryPool;

        navMeshAgent.updateRotation = false;            // 네미메쉬에서 회전 업데이트 X
    }
    private void OnEnable()
    {
        ChangeState(EnemyState.Idle);
    }
    private void OnDisable()
    {
        StopCoroutine(enemyState.ToString());
        enemyState = EnemyState.None;
    }
    public void ChangeState(EnemyState newState)
    {
        if (enemyState == newState) return;

        StopCoroutine(enemyState.ToString());
        enemyState = newState;
        StartCoroutine(enemyState.ToString());
    }
    private IEnumerator Idle()
    {
        StartCoroutine("AutoChangeFromIdleToWander");   // n초 후에 배회상태로 변경하는 코루틴
        while (true)                                                    // 대기 상태일 때 - 
        {
            CalculateDistanceToTargetAndSelectState();      // 플레이어와의 거리에 따라 행동 변경
            yield return null;
        }
    }
    private IEnumerator AutoChangeFromIdleToWander()
    {
        int changeTime = Random.Range(1, 5);
        
        yield return new WaitForSeconds(changeTime);

        ChangeState(EnemyState.Wander);
    }
    private IEnumerator Wander()
    {
        float currentTime = 0;
        float maxTime = 10;
        navMeshAgent.speed = status.WalkSpeed;                  // 이동속도 설정
        navMeshAgent.SetDestination(CalculateWanderPosition());     // 목표 위치 설정

        Vector3 to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);    // 목표 위치로 회전
        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.LookRotation(to - from);
        while (true)
        {
            currentTime += Time.deltaTime;

            to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);    // 플레이어에 근접하거나 일정시간동안 배회하면 - 
            from = new Vector3(transform.position.x, 0, transform.position.z);
            if((to-from).sqrMagnitude<0.01f || currentTime >= maxTime)
            {
                ChangeState(EnemyState.Idle);
            }
            CalculateDistanceToTargetAndSelectState();  // 플레이어와의 거리에 따라 행동 변경
            yield return null;
        }
    }
    private Vector3 CalculateWanderPosition()
    {
        float wanderRadius = 10;
        int wanderJitter = 0;
        int wanderJitterMin = 0;
        int wanderJitterMax = 360;
        Vector3 rangePosition = Vector3.zero;
        Vector3 rangeScale = Vector3.one * 100.0f;
        
        wanderJitter = Random.Range(wanderJitterMin, wanderJitterMax);
        Vector3 targetPosition = transform.position + SetAngle(wanderRadius, wanderJitter);

        targetPosition.x = Mathf.Clamp(targetPosition.x, rangePosition.x-rangeScale.x*0.5f, rangePosition.x+rangeScale.x*0.5f);
        targetPosition.y = 0.0f;
        targetPosition.z = Mathf.Clamp(targetPosition.z, rangePosition.z - rangeScale.z * 0.5f, rangePosition.z + rangeScale.z * 0.5f);
        return targetPosition;
    }
    private Vector3 SetAngle(float radius, int angle)
    {
        Vector3 position = Vector3.zero;
        position.x = Mathf.Cos(angle) * radius;
        position.z = Mathf.Sin(angle) * radius;
        return position;
    }
    private IEnumerator Pursuit()
    {
        while (true)
        {
            navMeshAgent.speed = status.RunSpeed;           // 상태에 따라 이동속도 변경
            navMeshAgent.SetDestination(target.position);     // 목표 위치를 플레이어로 설정
            LookRotationToTarget();                                 // 목표 방향을 주시함
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }
    private IEnumerator Attack()
    {
        navMeshAgent.ResetPath();       // 공격 시에는 정지(이동X)
        while (true)
        {
            LookRotationToTarget();     // 목표 방향 주시
            CalculateDistanceToTargetAndSelectState();
            if(Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;             // 공격속도 조절을 위한 시간 저장
                GameObject clone = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation); // 총알 생성
                clone.GetComponent<EnemyProjectile>().Setup(target.position);

            }
            yield return null;
        }
    }
    private void LookRotationToTarget()
    {
        Vector3 to = new Vector3(target.position.x, 0, target.position.z);      // 목표 위치
        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);    // 내 위치
        // 바로 돌기
        //transform.rotation = Quaternion.LookRotation(to - from);
        // 천천히 돌기
        Quaternion rotation = Quaternion.LookRotation(to - from);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.01f);
    }
    private void CalculateDistanceToTargetAndSelectState()
    {
        if (target == null) return;

        float distance = Vector3.Distance(target.position, transform.position);
        if (distance <= attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        else if (distance <= targetRecognitionRange)
        {
            ChangeState(EnemyState.Pursuit);
        }else if(distance>=pursuitLimitRange){
            ChangeState(EnemyState.Wander);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;     // 배회상태 경로
        Gizmos.DrawRay(transform.position, navMeshAgent.destination - transform.position);
        
        Gizmos.color = Color.green;     // 목표 인식 범위
        Gizmos.DrawWireSphere(transform.position, pursuitLimitRange);

        Gizmos.color = Color.gray;      // 추적 범위
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public void TakeDamage(int damage)
    {
        bool isDie = status.DecreaseHP(damage);
        if(isDie == true)
        {
            enemyMemoryPool.InactivateEnemy(gameObject);
        }
    }
}
