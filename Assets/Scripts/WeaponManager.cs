using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("게임 내 존재하는 모든 무기 목록 (자동 수집)")]
    public List<GunController> allGuns = new List<GunController>();

    [Header("현재 장착중인 무기")]
    public GunController currentGun;

    [Header("게임 시작 시 기본 무기 이름")]
    public string startWeaponName = "Pistol";  // ← 네 권총 이름 넣기

    void Awake()
    {
        // 자식 객체 중 GunController 전부 자동 수집
        GunController[] guns = GetComponentsInChildren<GunController>(true);

        foreach (var gun in guns)
        {
            allGuns.Add(gun);

            // 기본적으로 잠겨있으면 비활성화
            if (!gun.isUnlocked)
                gun.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // 시작 무기 자동 unlock + 장착
        GunController startGun = allGuns.Find(g => g.gameObject.name == startWeaponName);

        if (startGun != null)
        {
            startGun.isUnlocked = true;
            EquipGun(startGun);   // ★ 변경된 부분 → 장착 함수 호출
        }
    }

    // ---------------------------------------------
    //  ★ 핵심: 무기 장착 기능
    // ---------------------------------------------
    public void EquipGun(GunController targetGun)
    {
        if (targetGun == null || !targetGun.isUnlocked) return;

        foreach (var gun in allGuns)
            gun.gameObject.SetActive(false);

        targetGun.gameObject.SetActive(true);
        currentGun = targetGun;

        WeaponSwitcher switcher = GetComponent<WeaponSwitcher>();
        if (switcher != null)
        {
            int index = switcher.weapons.IndexOf(targetGun.gameObject);
            if (index == -1)
            {
                switcher.weapons.Add(targetGun.gameObject);
                index = switcher.weapons.Count - 1;
            }

            switcher.ForceSelect(index);
        }

        targetGun.ResetGunState(); // 장착 시 상태 초기화
    }


    // ---------------------------------------------
    //  무기 Unlock
    // ---------------------------------------------
    public void UnlockWeapon(WeaponData data)
    {
        GunController gun = GetGun(data);

        if (gun != null)
        {
            gun.isUnlocked = true;

            // Aura이면 즉시 활성화
            if (gun.isAuraMode)
                gun.gameObject.SetActive(true);

            // WeaponSwitcher에 새 무기 등록 요청
            WeaponSwitcher switcher = GetComponent<WeaponSwitcher>();
            if (switcher != null)
            {
                // weaponObj가 이미 리스트에 없으면 추가(안전)
                if (!switcher.weapons.Contains(gun.gameObject))
                {
                    switcher.UnlockWeapon(gun.gameObject);
                }
            }
        }
    }

    // ---------------------------------------------
    //  무기 데이터 조회 기능
    // ---------------------------------------------
    public GunController GetGun(WeaponData data)
    {
        return allGuns.Find(g => g.weaponData == data);
    }

    public bool IsWeaponUnlocked(WeaponData data)
    {
        GunController gun = GetGun(data);
        return gun != null && gun.isUnlocked;
    }

    // ---------------------------------------------
    //  무기 레벨 시스템
    // ---------------------------------------------
    public int GetWeaponLevel(WeaponData data)
    {
        GunController gun = GetGun(data);
        return gun ? gun.weaponLevel : 0;
    }

    public void LevelUpWeapon(WeaponData data)
    {
        GunController gun = GetGun(data);

        if (gun != null)
            gun.weaponLevel++;
    }

    public bool IsWeaponMaxLevel(WeaponData data)
    {
        GunController gun = GetGun(data);
        return gun != null && gun.IsMaxLevel();
    }

    // ---------------------------------------------
    //  전역 버프
    // ---------------------------------------------
    public float globalDamageBuff = 1f;
    public float globalFireRateBuff = 1f;

    public void ApplyGlobalDamageBuff(float value)
    {
        globalDamageBuff = value;
    }

    public void ApplyGlobalFireRateBuff(float value)
    {
        globalFireRateBuff = value;
    }
}
