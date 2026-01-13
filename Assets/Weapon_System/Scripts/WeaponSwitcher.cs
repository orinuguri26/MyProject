using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WeaponSwitcher : MonoBehaviour
{
    public int currentIndex = 0;
    public List<GameObject> weapons = new List<GameObject>();

    // Input Actions (VR Input)
    public InputActionProperty nextWeaponAction;
    public InputActionProperty prevWeaponAction;

    void Start()
    {
        // 자식 무기 전부 확인
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            GunController gun = obj.GetComponent<GunController>();

            if (gun == null) continue;
            if (gun.isAuraMode) continue;  // 아우라 무기 제외
            if (!gun.isUnlocked)
            {
                obj.SetActive(false);
                continue;
            }

            weapons.Add(obj); // 해금된 무기만 리스트에 추가
        }

        // 최소 1개라도 있다면 첫 무기 선택
        if (weapons.Count > 0)
            SelectWeapon(currentIndex);
    }

    void Update()
    {
        if (weapons.Count == 0) return;

        int previousIndex = currentIndex;

        if (nextWeaponAction.action.WasPressedThisFrame())
            currentIndex = (currentIndex + 1) % weapons.Count;

        if (prevWeaponAction.action.WasPressedThisFrame())
            currentIndex = (currentIndex - 1 + weapons.Count) % weapons.Count;

        if (previousIndex != currentIndex)
            SelectWeapon(currentIndex);
    }

    void SelectWeapon(int index)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            GameObject obj = weapons[i];
            bool isNowActive = (i == index);

            obj.SetActive(isNowActive);

            GunController gun = obj.GetComponent<GunController>();
            if (gun != null)
            {
                gun.isEquipped = isNowActive;

                if (isNowActive)
                {
                    // 무기 상태 초기화
                    gun.ResetGunState();

                    // UI 초기화
                    gun.UpdateAmmoUI();
                }
            }
        }
    }

    public void ForceSelect(int index)
    {
        if (index < 0 || index >= weapons.Count) return;
        currentIndex = index;
        SelectWeapon(index);
    }

    public void UnlockWeapon(GameObject weaponObj)
    {
        GunController gun = weaponObj.GetComponent<GunController>();
        if (gun == null) return;
        if (gun.isAuraMode) return; // 아우라 무기 제외

        gun.isUnlocked = true;

        // 이미 리스트에 있으면 패스
        if (weapons.Contains(weaponObj)) return;

        weapons.Add(weaponObj);

        // 새로 해금된 무기는 기본적으로 비활성화
        weaponObj.SetActive(false);
    }
}
