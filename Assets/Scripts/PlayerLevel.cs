using UnityEngine;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    [Header("레벨 설정")]
    public int level = 1;
    public float currentExp = 0;
    public float maxExp = 100;

    //public Text levelText;
    private PlayerController playerController;

    [Header("레벨 업 사운드")]
    public AudioClip levelUpSound; // [추가] 레벨업 효과음 파일
    public LevelUpManager levelUpManager;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void GainExp(float amount)
    {
        currentExp += amount;

        if (currentExp >= maxExp) LevelUp();

        if (playerController != null)
            playerController.UpdateExpUI(currentExp, maxExp);
    }

    void LevelUp()
    {
        level++;
        currentExp = 0;
        maxExp += 20f;

        if (levelUpSound != null) AudioSource.PlayClipAtPoint(levelUpSound, transform.position, 1.0f);

        // [핵심 변경] 여기서 매니저에게 "선택지 띄워라"고 명령합니다.
        if (levelUpManager != null)
        {
            levelUpManager.ApplyRandomUpgrade();
        }

        if (playerController != null)
            playerController.UpdateExpUI(currentExp, maxExp);
    }
}