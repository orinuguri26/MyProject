using UnityEngine;
using TMPro;

public class MedkitInventory : MonoBehaviour
{
    [Header("Medkit 설정")]
    public int medkitCount = 0;        // 보유 개수
    public float healAmount = 100f;    // 1회 회복량

    [Header("참조")]
    public PlayerHealth playerHealth;  // 체력 스크립트
    public TMP_Text medkitText;        // UI 텍스트

    [Header("사운드")]
    public AudioClip useSound;         // 메드킷 사용 사운드
    [Range(0f, 1f)] public float volume = 0.5f;
    private AudioSource audioSource;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        audioSource = GetComponent<AudioSource>(); // AudioSource 가져오기
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>(); // 없으면 추가

        UpdateUI();   // 시작할 때 UI 갱신
    }

    // ---------------- 메드킷 획득 ----------------
    public void AddMedkit(int amount)
    {
        medkitCount += amount;
        UpdateUI();   // UI 갱신
    }

    // ---------------- 메드킷 사용 ----------------
    public void UseMedkit()
    {
        if (medkitCount <= 0)
            return;

        medkitCount--;

        if (playerHealth != null)
            playerHealth.Heal(healAmount);

        // ---------------- 사운드 재생 ----------------
        if (useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(useSound, volume);
        }

        UpdateUI();   // UI 갱신
    }

    // ---------------- UI 갱신 ----------------
    private void UpdateUI()
    {
        if (medkitText != null)
            medkitText.text = "x" + medkitCount;
    }
}
