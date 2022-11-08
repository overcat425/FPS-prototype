using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponGrenade : WeaponBase
{
    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipFire;                // 공격 사운드

    [Header("Grenade")]
    [SerializeField]
    private GameObject grenadePrefab;           // 수류탄 프리팹
    [SerializeField]
    private Transform grenadeSpawnPoint;        // 수류탄 스폰 위치 설정
    private void OnEnable()
    {
        onAmmoPackEvent.Invoke(weaponSettings.currentAmmoPack);
        onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);
    }
    private void Awake()
    {
        base.Setup();
        weaponSettings.currentAmmoPack = weaponSettings.maxAmmoPack;
        weaponSettings.currentAmmo = weaponSettings.maxAmmo;
    }
    public override void StartWeapon(int type = 0)
    {
        if(type == 0 && isAttack == false && weaponSettings.currentAmmo > 0)
        {
            StartCoroutine("OnAttack");
        }
    }
    public override void StopWeapon(int type = 0)
    {
    }
    public override void StartReload()
    {
    }
    public IEnumerator OnAttack()
    {
        isAttack = true;
        animator.Play("Fire", -1, 0);
        PlaySound(audioClipFire);
        yield return new WaitForEndOfFrame();
        while (true)
        {
            if (animator.CurrentAnimationIs("Movement"))
            {
                isAttack=false;
                yield break;
            }
            yield return null;
        }
    }
    public void SpawnGrenadeProjectile()
    {
        GameObject grenadeClone = Instantiate(grenadePrefab, grenadeSpawnPoint.position, Random.rotation);
        grenadeClone.GetComponent<WeaponGrenadeProjectile>().Setup(weaponSettings.damage, transform.parent.forward);
        weaponSettings.currentAmmo--;
        onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);
    }
    public override void IncreaseAmmoPack(int ammo)
    {
        weaponSettings.currentAmmo = weaponSettings.currentAmmo + ammo > weaponSettings.maxAmmo ? weaponSettings.maxAmmo : weaponSettings.currentAmmo + ammo;
        onAmmoEvent.Invoke(weaponSettings.currentAmmo, weaponSettings.maxAmmo);
    }
}