using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Ammo UI")]
    public Text ammoText;
    public int currentAmmo = 10;

    [Header("Stamina UI")]
    public Image staminaBar;
    public float maxStamina = 3f;       // 최대 달리기 시간 (초)
    public float recoveryRate = 1f;     // 초당 회복 속도

    [Header("EXP UI")]
    public Image expBar;

    private float currentStamina;
    private bool isRunning = false;

    void Start()
    {
        currentStamina = maxStamina;
        UpdateAmmoUI();
        UpdateStaminaUI();
    }

    void Update()
    {
        HandleStamina();
    }
    #region Stamina
    private void HandleStamina()
    {
        if (isRunning)
        {
            // 달리기 중이면 스테미너 감소
            currentStamina -= Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isRunning = false; // 스테미너 다 떨어지면 달리기 제한
            }
        }
        else
        {
            // 달리기 안하면 회복
            if (currentStamina < maxStamina)
            {
                currentStamina += recoveryRate * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }

        UpdateStaminaUI();
    }
    public void SetRunning(bool running)
    {
        isRunning = running && currentStamina > 0f;
    }

    public float CurrentStamina => currentStamina;

    private void UpdateStaminaUI()
    {
        if (staminaBar != null)
            staminaBar.fillAmount = currentStamina / maxStamina;
    }
    public void UpdateExpUI(float currentExp, float maxExp)
    {
        if (expBar != null)
            expBar.fillAmount = currentExp / maxExp;
    }

    #endregion

    #region Ammo
    public void ConsumeAmmo(int amount)
    {
        currentAmmo -= amount;
        if (currentAmmo < 0) currentAmmo = 0;
        UpdateAmmoUI();
    }

    public void SetAmmo(int amount)
    {
        currentAmmo = Mathf.Max(0, amount);
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = $"Ammo: {currentAmmo}";
    }
    #endregion
}
