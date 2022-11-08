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
    private float targetRecognitionRange = 30;      // ���� �÷��̾ �����ϴ� �Ÿ�
    [SerializeField]
    private float pursuitLimitRange = 10;               // �Ÿ��� 10�̻� �־����� ��ȸ���·� ��ȯ

    [Header("Attack")]
    [SerializeField]
    private GameObject projectilePrefab;            // �Ѿ� ������
    [SerializeField]
    private Transform projectileSpawnPoint;         // �Ѿ� ��������Ʈ
    [SerializeField]
    private float attackRange = 30;                     // �� ���� ��Ÿ�
    [SerializeField]
    private float attackRate = 0.5f;                    // �� ���ݼӵ�

    [Header("Audioclips")]
    [SerializeField]
    private AudioClip audioclipfire;
    private AudioSource audioSource;

    private EnemyState enemyState = EnemyState.None;
    private float lastAttackTime = 0;

    private PlayerStatus status;                                // �̵��ӵ�
    private NavMeshAgent navMeshAgent;                  // ����Ƽ �׺���̼��� �̿��� �� �̵�����
    private Transform target;                                   // ���� ���� ���(�÷��̾�)
    private EnemyMemoryPool enemyMemoryPool;

    //private void Awake()
    public void Setup(Transform target, EnemyMemoryPool enemyMemoryPool)
    {
        status = GetComponent<PlayerStatus>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        this.target = target;
        this.enemyMemoryPool = enemyMemoryPool;

        navMeshAgent.updateRotation = false;            // �׹̸޽����� ȸ�� ������Ʈ X
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
        StartCoroutine("AutoChangeFromIdleToWander");   // n�� �Ŀ� ��ȸ���·� �����ϴ� �ڷ�ƾ
        while (true)                                                    // ��� ������ �� - 
        {
            CalculateDistanceToTargetAndSelectState();      // �÷��̾���� �Ÿ��� ���� �ൿ ����
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
        navMeshAgent.speed = status.WalkSpeed;                  // �̵��ӵ� ����
        navMeshAgent.SetDestination(CalculateWanderPosition());     // ��ǥ ��ġ ����

        Vector3 to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);    // ��ǥ ��ġ�� ȸ��
        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);
        transform.rotation = Quaternion.LookRotation(to - from);
        while (true)
        {
            currentTime += Time.deltaTime;

            to = new Vector3(navMeshAgent.destination.x, 0, navMeshAgent.destination.z);    // �÷��̾ �����ϰų� �����ð����� ��ȸ�ϸ� - 
            from = new Vector3(transform.position.x, 0, transform.position.z);
            if((to-from).sqrMagnitude<0.01f || currentTime >= maxTime)
            {
                ChangeState(EnemyState.Idle);
            }
            CalculateDistanceToTargetAndSelectState();  // �÷��̾���� �Ÿ��� ���� �ൿ ����
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
            navMeshAgent.speed = status.RunSpeed;           // ���¿� ���� �̵��ӵ� ����
            navMeshAgent.SetDestination(target.position);     // ��ǥ ��ġ�� �÷��̾�� ����
            LookRotationToTarget();                                 // ��ǥ ������ �ֽ���
            CalculateDistanceToTargetAndSelectState();
            yield return null;
        }
    }
    private IEnumerator Attack()
    {
        navMeshAgent.ResetPath();       // ���� �ÿ��� ����(�̵�X)
        while (true)
        {
            LookRotationToTarget();     // ��ǥ ���� �ֽ�
            CalculateDistanceToTargetAndSelectState();
            if(Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;             // ���ݼӵ� ������ ���� �ð� ����
                GameObject clone = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation); // �Ѿ� ����
                clone.GetComponent<EnemyProjectile>().Setup(target.position);

            }
            yield return null;
        }
    }
    private void LookRotationToTarget()
    {
        Vector3 to = new Vector3(target.position.x, 0, target.position.z);      // ��ǥ ��ġ
        Vector3 from = new Vector3(transform.position.x, 0, transform.position.z);    // �� ��ġ
        // �ٷ� ����
        //transform.rotation = Quaternion.LookRotation(to - from);
        // õõ�� ����
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
        Gizmos.color = Color.black;     // ��ȸ���� ���
        Gizmos.DrawRay(transform.position, navMeshAgent.destination - transform.position);
        
        Gizmos.color = Color.green;     // ��ǥ �ν� ����
        Gizmos.DrawWireSphere(transform.position, pursuitLimitRange);

        Gizmos.color = Color.gray;      // ���� ����
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
