using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemAmmoPack : ItemBase
{
    [SerializeField]
    private GameObject ammoPackEffectPrefab;
    [SerializeField]
    private int increaseAmmoPack = 1;
    private float rotateSpeed = 50;

    private IEnumerator Start()
    {
        while (true)
        {
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }
    public override void Use(GameObject entity)
    {
        entity.GetComponent<WeaponSwitchingSystem>().IncreaseAmmoPack(WeaponType.Main, increaseAmmoPack);
        Instantiate(ammoPackEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
