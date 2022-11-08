using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitchingSystem : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;
    [SerializeField]
    private PlayerHUD playerHUD;
    [SerializeField]
    private WeaponBase[] weapons;                       // 무기 종류

    private WeaponBase currentWeapon;               // 현재 무기
    private WeaponBase previousWeapon;              // 바로 전에 사용한 무기

    private void Awake()
    {
        playerHUD.SetupAllWeapons(weapons);
        for(int i = 0; i<weapons.Length; ++i)
        {
            if (weapons[i].gameObject != null)
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
        SwitchingWeapon(WeaponType.Main);
    }
    private void Update()
    {
        UpdateSwitch();
    }
    private void UpdateSwitch()
    {
        if (!Input.anyKeyDown) return;
        int inputIndex = 0;
        if(int.TryParse(Input.inputString, out inputIndex)&&(inputIndex>0 && inputIndex < 5))
        {
            SwitchingWeapon((WeaponType)(inputIndex - 1));
        }
    }
    private void SwitchingWeapon(WeaponType weaponType)
    {
        if (weapons[(int)weaponType] == null)
        {
            return;
        }
        if(currentWeapon != null)
        {
            previousWeapon = currentWeapon;
        }
        currentWeapon = weapons[(int)weaponType];
        if(currentWeapon == previousWeapon)
        {
            return;
        }
        playerController.SwitchingWeapon(currentWeapon);
        playerHUD.SwitchngWeapon(currentWeapon);
        if(previousWeapon != null)
        {
            previousWeapon.gameObject.SetActive(false);
        }
        currentWeapon.gameObject.SetActive(true);
    }
    public void IncreaseAmmoPack(WeaponType weaponType, int AmmoPack)
    {
        if (weapons[(int)weaponType] != null)       // 해당 무기가 있으면
        {
            weapons[(int)weaponType].IncreaseAmmoPack(AmmoPack);    // 탄창수 증가
        }
    }
    public void IncreaseAmmoPack(int ammoPack)
    {
        for(int i = 0; i<weapons.Length; ++i)
        {
            if (weapons[i] != null)
            {
                weapons[i].IncreaseAmmoPack(ammoPack);
            }
        }
    }
}
