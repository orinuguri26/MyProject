using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("참조 스크립트")]
    public PlayerHealth healthScript;
    public PlayerController moveScript;
    public WeaponManager weaponManager;

    [Header("스탯 정보")]
    public float healthRegen = 0f; // 초당 체력 회복량

    [Header("추가 연결 (자석)")]
    public Transform magnetArea; // 인스펙터에서 'MagnetArea' 오브젝트를 넣으세요
    private SphereCollider magnetCollider; // 실제 범위를 담당하는 콜라이더

    void Start()
    {
        if (magnetArea != null)
        {
            magnetCollider = magnetArea.GetComponent<SphereCollider>();
        }
    }
    void Update()
    {
        if (healthRegen > 0)
        {
            // TakeDamage에 음수를 넣으면 회복됨 (-값 * 시간)
            healthScript.TakeDamage(-healthRegen * Time.deltaTime);
        }
    }

    public void ApplyUpgrade(UpgradeData data)
    {
        // --------------------------------------------------------
        // 1. [무기 해금] & [무기 레벨업]
        // --------------------------------------------------------
        if (data.type.ToString().ToLower().StartsWith("unlock"))
        {
            // [변경] WeaponManager에게 해금 명령
            if (weaponManager != null && data.relatedWeapon != null)
                weaponManager.UnlockWeapon(data.relatedWeapon);
            return;
        }

        if (data.relatedWeapon != null)
        {
            // [변경] WeaponManager를 통해 해당 총을 찾아서 레벨업
            GunController gun = weaponManager.GetGun(data.relatedWeapon);
            if (gun != null)
            {
                gun.LevelUpWeapon();
            }
            return;
        }

        // --------------------------------------------------------
        // 2. [스탯 강화]
        // --------------------------------------------------------

        float percent = data.value / 100f;

        switch (data.type)
        {
            case UpgradeType.Stat_MaxHealth:
                healthScript.maxHealth += data.value;
                healthScript.TakeDamage(-data.value); // 회복
                Debug.Log("최대 체력 증가!");
                break;

            case UpgradeType.Stat_MoveSpeed:
                float increaseSpeed = moveScript.moveSpeed * percent;
                moveScript.moveSpeed += increaseSpeed;
                Debug.Log($"이동 속도 {data.value}% 증가!");
                break;

            case UpgradeType.Stat_HealthRegen:
                healthRegen += data.value; // 예: value가 1이면 초당 1 회복
                Debug.Log($"초당 체력 재생 +{data.value} 증가!");
                break;

            case UpgradeType.Stat_MagnetRange:
                if (magnetCollider != null)
                {
                    // 현재 반지름에서 퍼센트만큼 증가
                    magnetCollider.radius += magnetCollider.radius * percent;
                    Debug.Log($"자석 범위 {data.value}% 증가! (현재: {magnetCollider.radius:F1})");
                }
                break;

            case UpgradeType.Weapon_Damage:
                // [변경] 매니저가 관리하는 모든 총 리스트(allGuns)를 순회
                foreach (var gun in weaponManager.allGuns)
                {
                    gun.globalDamageBuff += percent;
                }
                Debug.Log($"모든 무기 공격력 {data.value}% 증가!");
                break;

            case UpgradeType.Weapon_FireRate:
                // [변경] 매니저가 관리하는 모든 총 리스트(allGuns)를 순회
                foreach (var gun in weaponManager.allGuns)
                {
                    gun.globalFireRateBuff += percent;
                }
                Debug.Log($"모든 무기 공속 {data.value}% 증가!");
                break;
        }
    }
}