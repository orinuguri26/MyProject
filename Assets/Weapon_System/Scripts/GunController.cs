using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

[System.Serializable]
public class WeaponLevelData
{
    public float damageIncrease = 0f;
    public float fireRateIncrease = 0f;
    public int maxAmmoIncrease = 0;
    public float rangeIncrease = 0f; //오라 범위
    public int pelletCountIncrease = 0;
    public int penetrationIncrease = 0;
}

public class GunController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text ammoText;

    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    private AudioSource audioSource;

    [Header("VFX")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("무기 기본 성능")]
    public float baseDamage = 10f;
    public float baseFireRate = 0.2f;
    public int baseMaxAmmo = 30;
    public int basePelletCount = 1;
    public int basePenetration = 0;
    public float reloadTime = 1.5f;

    [Header("VR Input Actions")]
    public InputActionProperty fireAction;      // 오른손 트리거
    public InputActionProperty reloadAction;    // 오른손 그립

    [Header("레벨 시스템")]
    public int weaponLevel = 1;
    public List<WeaponLevelData> levelDataList;

    [Header("레벨업 보너스 (무기 전용 보너스)")]
    public float bonusDamage = 0f;
    public float bonusFireRatePercent = 0f;
    public int bonusMaxAmmo = 0;
    public int bonusPelletCount = 0;
    public int bonusPenetration = 0;
    public float bonusRange = 0f; // [추가] 누적된 범위 보너스

    [Header("전역 버프 (카드 시스템에서 오르는 버프)")]
    public float globalDamageBuff = 0f;        // 모든 무기 데미지 %
    public float globalFireRateBuff = 0f;      // 모든 무기 발사속도 %

    [Header("연발 설정")]
    public int burstCount = 1;                 // 1 = 단발, 3 = 3점사 등
    public float burstDelay = 0.07f;           // 연발 사이 간격

    [Header("샷건 설정")]
    public bool isShotgun = false;
    public float spreadAngle = 15f;

    [Header("모드 설정")]
    public bool isAutoMode = false;
    public float autoRange = 0.5f; // 이게 기본 범위(반지름)
    public float aimOffset = 0.2f;

    [Header("오라(장판) 무기 설정")]
    public bool isAuraMode = false;
    public Transform auraVisual;
    public float auraRotateSpeed = 30f;

    private SphereCollider auraCollider;
    private List<Enemy> enemiesInAura = new List<Enemy>();
    private float auraDamageTimer = 0f;

    [Header("기타")]
    public WeaponData weaponData;
    public bool isUnlocked = true;

    public int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private Vector3 original;

    public AudioClip useSound;
    public bool isEquipped = false;
    void OnEnable()
    {
        isReloading = false;
        UpdateAmmoUI();

        if (!isAuraMode)
        {
            fireAction.action?.Enable();
            reloadAction.action?.Enable();
        }
        if (isAuraMode && auraVisual != null)
        {
            auraVisual.gameObject.SetActive(true);
        }
    }
    void OnDisable()
    {
        if (isAuraMode && auraVisual != null)
        {
            auraVisual.gameObject.SetActive(false);
        }
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = GetFinalMaxAmmo();
        UpdateAmmoUI();

        if (isAuraMode && auraVisual != null)
        {
            auraCollider = GetComponent<SphereCollider>();
            if (auraCollider == null)
            {
                auraCollider = gameObject.AddComponent<SphereCollider>();
                auraCollider.isTrigger = true;
            }
        }
    }

    void Update()
    {
        if (!isUnlocked)
        {
            if (auraVisual != null) auraVisual.gameObject.SetActive(false);
            return;
        }
        if (isAuraMode)
        {
            AuraLogic();
            return;
        }
        else if (isReloading) return;

        HandleReloadInput();
        HandleFireInput();
    }
    void AuraLogic()
    {
        if (auraVisual == null || auraCollider == null) return;

        // 1. 범위
        float finalRange = autoRange + bonusRange;

        // 2. 위치 고정 (플레이어 발밑 - 카메라 기준)
        Vector3 camPos = Camera.main.transform.position;
        auraVisual.position = new Vector3(
            camPos.x,
            0.1f,
            camPos.z);
        
        // 3. 회전
                auraVisual.Rotate(Vector3.up * auraRotateSpeed * Time.deltaTime);

        // 4. 비주얼 크기 변경
        auraVisual.localScale = new Vector3(finalRange * 2f, 0.05f, finalRange * 2f);

        // 5. 콜라이더 크기 변경
        auraCollider.radius = finalRange / 3f;

        // 6. 틱 데미지
        auraDamageTimer += Time.deltaTime;

        float tickRate = GetFinalFireRate();
        if (tickRate < 0.1f) tickRate = 0.1f;

        if (auraDamageTimer >= tickRate)
        {
            DealAuraDamage();
            auraDamageTimer = 0f;
        }

        enemiesInAura.RemoveAll(e => e == null);
    }

    void DealAuraDamage()
    {
        float damage = GetFinalDamage();
        if (useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(useSound);
        }
        foreach (var enemy in enemiesInAura)
        {
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (!isAuraMode) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null && !enemiesInAura.Contains(enemy))
        {
            enemiesInAura.Add(enemy);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (!isAuraMode) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null && enemiesInAura.Contains(enemy))
        {
            enemiesInAura.Remove(enemy);
        }
    }
    // ---------------- Reload Input ----------------
    private void HandleReloadInput()
    {
        if (reloadAction.action.WasPressedThisFrame())
        {
            if (!isReloading)
                StartCoroutine(Reload());
        }
    }

    // ---------------- Fire Input ----------------
    private void HandleFireInput()
    {
        if (!fireAction.action.IsPressed()) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + GetFinalFireRate();
        StartCoroutine(FireBurst());
    }

    // ---------------- 연발 (버스트) ----------------
    private IEnumerator FireBurst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            FireOnce();

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }
    }

    // ---------------- Shot / Single ----------------
    private void FireOnce()
    {
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        currentAmmo--;
        UpdateAmmoUI();

        PlaySound(fireSound);
        PlayMuzzleFlash();

        if (isShotgun)
            FireShotgun();
        else
            FireNormal();

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    private void FireNormal()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        BulletController bc = bulletObj.GetComponent<BulletController>();

        if (bc != null)
        {
            bc.damage = GetFinalDamage();
            bc.penetration = GetFinalPenetration();
        }
        Debug.Log("Bullet Damage = " + bc.damage);
    }

    private void FireShotgun()
    {
        int pelletCount = GetFinalPelletCount();

        for (int i = 0; i < pelletCount; i++)
        {
            float x = Random.Range(-spreadAngle, spreadAngle);
            float y = Random.Range(-spreadAngle, spreadAngle);
            Quaternion spreadRot = firePoint.rotation * Quaternion.Euler(x, y, 0);

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, spreadRot);
            BulletController bc = bulletObj.GetComponent<BulletController>();

            if (bc != null)
            {
                bc.damage = GetFinalDamage();
                bc.penetration = GetFinalPenetration();
            }
        }
    }

    // ---------------- Reload ----------------
    private IEnumerator Reload()
    {
        isReloading = true;

        PlaySound(reloadSound);
        SetAmmoUIText("Reload...");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = GetFinalMaxAmmo();
        isReloading = false;
        UpdateAmmoUI();
    }

    // ---------------- UI ----------------
    public void UpdateAmmoUI()
    {
        if (!isEquipped) return;
        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {GetFinalMaxAmmo()}";
    }

    private void SetAmmoUIText(string text)
    {
        if (ammoText != null)
            ammoText.text = text;
    }

    // ---------------- Sound & VFX ----------------
    private void PlayMuzzleFlash()
    {
        if (muzzleFlashPrefab == null) return;

        GameObject fx = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
        fx.transform.SetParent(firePoint);
        Destroy(fx, 0.5f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
    public void ResetGunState()
    {
        isReloading = false;
        nextFireTime = 0f;
        //isEquipped = true;              // 무기 장착 시 활성화
        //fireAction.action?.Enable();    // 트리거 Enable
        //reloadAction.action?.Enable();  // 그립 Enable
        //UpdateAmmoUI();
    }

    // ---------------- Level System ----------------
    public bool IsMaxLevel()
    {
        return weaponLevel >= levelDataList.Count + 1;
    }

    public string GetNextLevelTooltip()
    {
        int index = weaponLevel - 1;
        if (index >= levelDataList.Count) return "Max Level";

        WeaponLevelData d = levelDataList[index];

        if (!isAuraMode)
        {
            return
                $"데미지 +{d.damageIncrease}\n" +
                $"공격속도 +{d.fireRateIncrease}%\n" +
                $"장탄수 +{d.maxAmmoIncrease}\n" +
                $"산탄 +{d.pelletCountIncrease}\n" +
                $"관통 +{d.penetrationIncrease}";
        }
        else
        {
            return
                $"데미지 +{d.damageIncrease}\n" +
                $"공격속도 +{d.fireRateIncrease}%\n" +
                $"공격범위 +{d.rangeIncrease}";

        }
    }

    public void LevelUpWeapon()
    {
        if (IsMaxLevel()) return;

        int index = weaponLevel - 1;
        WeaponLevelData data = levelDataList[index];
        weaponLevel++;

        bonusDamage += data.damageIncrease;
        bonusFireRatePercent += data.fireRateIncrease;
        bonusMaxAmmo += data.maxAmmoIncrease;
        bonusPelletCount += data.pelletCountIncrease;
        bonusPenetration += data.penetrationIncrease;
        bonusRange += data.rangeIncrease;
        currentAmmo = GetFinalMaxAmmo();
        UpdateAmmoUI();
    }

    // ---------------- Final Stat Calculation ----------------
    public float GetFinalDamage() =>
        (baseDamage + bonusDamage) * (1f + globalDamageBuff);

    public float GetFinalFireRate() =>
        (baseFireRate / (1f + (bonusFireRatePercent + globalFireRateBuff) / 100f));

    public int GetFinalMaxAmmo() =>
        baseMaxAmmo + bonusMaxAmmo;

    public int GetFinalPelletCount() =>
        basePelletCount + bonusPelletCount;

    public int GetFinalPenetration() =>
        basePenetration + bonusPenetration;
}
