using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject levelUpPanel;
    public Text titleText;
    public Text descText;

    [Header("데이터")]
    public UpgradeData[] allUpgrades;

    [Header("플레이어")]
    public PlayerStats playerStats;

    [Header("UI 표시 시간")]
    public float displayDuration = 2f;

    void Start()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    public void ApplyRandomUpgrade()
    {
        // 가능한 업그레이드 리스트
        List<UpgradeData> unlockable = new List<UpgradeData>();
        foreach (var u in allUpgrades)
            if (IsCardUnlockable(u))
                unlockable.Add(u);

        if (unlockable.Count == 0)
            return;

        UpgradeData chosen = unlockable[Random.Range(0, unlockable.Count)];

        // 업그레이드 적용
        playerStats.ApplyUpgrade(chosen);

        // UI 표시
        if (levelUpPanel != null)
        {
            titleText.text = chosen.title;

            string finalDesc = chosen.description;

            // 무기 관련 카드라면 GunController의 GetNextLevelTooltip 내용 추가
            if (chosen.relatedWeapon != null)
            {
                GunController gun = playerStats.weaponManager.GetGun(chosen.relatedWeapon);
                if (gun != null)
                {
                    // 현재 레벨 기준 다음 레벨 업그레이드 정보
                    finalDesc += "\n" + gun.GetNextLevelTooltip();
                }
            }

            descText.text = finalDesc;

            Transform cam = Camera.main.transform;
            levelUpPanel.transform.position = cam.position + cam.forward * 1.5f;
            levelUpPanel.transform.rotation = Quaternion.LookRotation(cam.forward, Vector3.up);
            levelUpPanel.SetActive(true);

            Invoke(nameof(HidePanel), displayDuration);
        }
    }

    void HidePanel()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    bool IsCardUnlockable(UpgradeData data)
    {
        if (data.relatedWeapon == null) return true;

        bool isUnlocked = playerStats.weaponManager.IsWeaponUnlocked(data.relatedWeapon);
        bool isMaxLevel = playerStats.weaponManager.IsWeaponMaxLevel(data.relatedWeapon);
        string typeName = data.type.ToString().ToLower();

        if (typeName.StartsWith("unlock"))
            return !isUnlocked;

        if (isUnlocked && !isMaxLevel)
            return true;

        return false;
    }
}
