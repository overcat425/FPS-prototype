using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponKnifeCollider : MonoBehaviour
{
    [SerializeField]
    private HitScanMemoryPool HitScanMemoryPool;
    [SerializeField]
    private Transform KnifeTransform;

    private new Collider collider;
    private int damage;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        collider.enabled = false;
    }
    public void StartCollider(int damage)
    {
        this.damage = damage;
        collider.enabled = true;
        StartCoroutine("DisablebyTime", 0.1f);
    }
    private IEnumerator DisablebyTime(float time)
    {
        yield return new WaitForSeconds(time);
        collider.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        HitScanMemoryPool.SpawnHitScan(other, KnifeTransform);

        if (other.CompareTag("HitScanEnemy"))
        {
            other.GetComponentInParent<EnemyFSM>().TakeDamage(damage);
        }else if (other.CompareTag("InterationcObject"))
        {
            other.GetComponent<InteractionObject>().TakeDamage(damage);
        }
    }
}
