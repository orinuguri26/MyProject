using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Image healthBar;
    public GameObject gameOverUI;     // 게임오버 패널

    [Header("VR 상호작용")]
    public InputActionProperty leftTriggerAction; // 왼손 트리거
    private bool wasPressed = false;

    private bool isGameOver = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        if (gameOverUI != null)
            gameOverUI.SetActive(false);

    }

    void Update()
    {
        if (isGameOver)
        {
            bool isPressed = leftTriggerAction.action.IsPressed();
            if (isPressed && !wasPressed)
            {
                RetryGame();
            }
            wasPressed = isPressed;
        }

    }

    public void TakeDamage(float amount)
    {
        if (isGameOver) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;
    }

    private void Die()
    {
        isGameOver = true;

        GamePauseManager.RequestPause();
        // 카메라 앞에 UI 자동 배치
        Transform cam = Camera.main.transform;
        gameOverUI.transform.position = cam.position + cam.forward * 0.6f;
        gameOverUI.transform.rotation = Quaternion.LookRotation(cam.forward, Vector3.up);
        //gameOverUI.transform.position += new Vector3(0f, -0.1f, 0f);
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

    }

    // 버튼에서 호출 가능 (Retry 버튼)
    public void RetryGame()
    {
        GamePauseManager.ReleasePause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Heal(float amount)
    {
        if (isGameOver) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();
    }
}
