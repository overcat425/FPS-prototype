using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponKnife : WeaponBase
{
    [SerializeField]
    private WeaponKnifeCollider weaponKnifeCollider;
    private void OnEnable()
    {
        isAttack = false;
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
        if (isAttack == true) return;
        if(weaponSettings.isAutomaticAttack == true)
        {
            StartCoroutine("OnAttackLoop", type);
        }
        else
        {
            StartCoroutine("OnAttack", type);
        }
    }
    public override void StopWeapon(int type = 0)
    {
        isAttack = false;
        StopCoroutine("OnAttackLoop");
    }
    public override void StartReload()
    {
    }
    private IEnumerator OnAttackLoop(int type)
    {
        while (true)
        {
            yield return StartCoroutine("OnAttack", type);
        }
    }
    private IEnumerator OnAttack(int type)
    {
        isAttack = true;
        animator.SetFloat("attacktype", type);
        animator.Play("Fire", -1, 0);
        yield return new WaitForEndOfFrame();
        while (true)
        {
            if (animator.CurrentAnimationIs("Movement"))
            {
                isAttack = false;
                yield break;
            }
            yield return null;
        }
    }
    public void StartWeaponKnifeCollider()
    {
        weaponKnifeCollider.StartCollider(weaponSettings.damage);
    }
}
