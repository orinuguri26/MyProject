using UnityEngine;

public enum UpgradeType
{
    Stat_MaxHealth,    // 체력 증가
    Stat_MoveSpeed,    // 이동 속도 증가
    Weapon_Damage,     // 무기 데미지 증가 (전체)
    Weapon_FireRate,   // 연사 속도 증가 (전체)
    Unlock_Shotgun,    // (예시) 무기 해금
    Unlock_Sniper,
    Unlock_Aura,
    LevelUp_HandGun,    // 특정 무기 레벨업
    LevelUp_Shotgun,
    LevelUp_Sniper,
    LevelUp_Aura,
    Stat_MagnetRange,
    Stat_HealthRegen

}

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string title;       // 화면에 뜰 이름 (예: "강화된 총알")
    [TextArea]
    public string description; // 설명

    public WeaponData relatedWeapon;

    public UpgradeType type;   // 업그레이드 종류
    public float value;        // 올라가는 수치 (예: 10)
}