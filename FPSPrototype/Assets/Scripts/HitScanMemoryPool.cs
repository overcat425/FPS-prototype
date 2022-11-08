using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitScanType { Normal = 0, Obstacle, Enemy, InteractionObject, }
public class HitScanMemoryPool : MonoBehaviour
{
    [SerializeField]
    private GameObject[] HitScanPrefab;
    private MemoryPool[] memoryPool;

    private void Awake()
    {
        memoryPool = new MemoryPool[HitScanPrefab.Length];
        for(int i = 0; i < memoryPool.Length; ++i)
        {
            memoryPool[i] = new MemoryPool(HitScanPrefab[i]);
        }
    }
    public void SpawnHitScan(RaycastHit hit)
    {
        if (hit.transform.CompareTag("HitScanNormal"))
        {
            OnSpawnHitScan(HitScanType.Normal, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else if (hit.transform.CompareTag("HitScanObstacle"))
        {
            OnSpawnHitScan(HitScanType.Obstacle, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else if (hit.transform.CompareTag("HitScanEnemy"))
        {
            OnSpawnHitScan(HitScanType.Enemy, hit.point, Quaternion.LookRotation(hit.normal));
        }else if (hit.transform.CompareTag("InteractionObject"))
        {
            Color color = hit.transform.GetComponentInChildren<MeshRenderer>().material.color;
            OnSpawnHitScan(HitScanType.InteractionObject, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
    public void SpawnHitScan(Collider other, Transform knifeTransform)
    {
        if (other.CompareTag("HitScanNormal"))
        {
            OnSpawnHitScan(HitScanType.Normal, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation));
        }else if (other.CompareTag("HitScanObstacle"))
        {
            OnSpawnHitScan(HitScanType.Obstacle, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation));
        }else if (other.CompareTag("HitScanEnemy"))
        {
            OnSpawnHitScan(HitScanType.Enemy, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation));
        }
        else if (other.CompareTag("InteractionObject"))
        {
            Color color = other.transform.GetComponentInChildren<MeshRenderer>().material.color;
            OnSpawnHitScan(HitScanType.InteractionObject, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation), color);
        }
    }
    public void OnSpawnHitScan(HitScanType type, Vector3 position, Quaternion rotation, Color color = new Color())
    {
        GameObject item = memoryPool[(int)type].ActivatePoolItem();
        item.transform.position = position;
        item.transform.rotation = rotation;
        item.GetComponent<Impact>().Setup(memoryPool[(int)type]);
        if(type == HitScanType.InteractionObject)
        {
            ParticleSystem.MainModule main = item.GetComponent<ParticleSystem>().main;
            main.startColor = color;
        }
    }
}
