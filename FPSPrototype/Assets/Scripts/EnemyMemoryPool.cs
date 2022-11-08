using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private GameObject enemySpawnPointPrefab;   // 적 등장 전 위치알림
    [SerializeField]
    private GameObject enemyPrefab;                 // 적 프리팹
    [SerializeField]
    private float enemySpawnTime = 60;
    [SerializeField]
    private float enemySpawnLatency = 1;

    private MemoryPool spawnPointMemoryPool;
    private MemoryPool enemyMemoryPool;

    private int numberOfEnemiesSpawnedAtOnce = 10;
    private Vector3Int mapSize = new Vector3Int(50,5,50);       // 맵 크기 벡터

    private void Awake()
    {
        spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrefab);
        enemyMemoryPool = new MemoryPool(enemyPrefab);

        StartCoroutine("SpawnTile");
    }
    private IEnumerator SpawnTile()
    {
        int currentNumber = 0;
        int maximumNumber = 50;
        while (true)
        {
            for(int i = 0; i < numberOfEnemiesSpawnedAtOnce; ++i)
            {
                GameObject item = spawnPointMemoryPool.ActivatePoolItem(); 
                item.transform.position = new Vector3(Random.Range(-mapSize.x*0.49f, mapSize.x*0.49f), 1, Random.Range(-mapSize.y*0.49f, mapSize.y*0.49f));
                StartCoroutine("SpawnEnemy", item);
            }
            currentNumber++;
            if (currentNumber >= maximumNumber)
            {
                currentNumber = 0;
                numberOfEnemiesSpawnedAtOnce++;
            }
            yield return new WaitForSeconds(enemySpawnTime);
        }
    }
    private IEnumerator SpawnEnemy(GameObject point)
    {
        yield return new WaitForSeconds(enemySpawnLatency);
        GameObject item = enemyMemoryPool.ActivatePoolItem();   // 적 생성, 위치 설정
        item.transform.position = point.transform.position;
        item.GetComponent<EnemyFSM>().Setup(target, this);

        spawnPointMemoryPool.InactivatePoolItem(point);         // 타일 오브젝트 비활성화
    }
    public void InactivateEnemy(GameObject enemy)
    {
        enemyMemoryPool.InactivatePoolItem(enemy);
    }
}
