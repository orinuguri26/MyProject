using UnityEngine;

public class ExpGem : MonoBehaviour
{
    public float expAmount = 10f;
    public float moveSpeed = 8f;

    // [추가] 획득 소리 파일
    public AudioClip pickupSound;
    // [추가] 소리 크기 (0.0 ~ 1.0)
    [Range(0f, 1f)] public float volume = 0.5f;

    private Transform targetPlayer;
    private bool isMagnetized = false;

    void Update()
    {
        if (isMagnetized && targetPlayer != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, moveSpeed * Time.deltaTime);

            // 플레이어 거리 체크하여 자동으로 먹기
            if (Vector3.Distance(transform.position, targetPlayer.position) < 0.5f)
            {
                EatGem();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Magnet"))
        {
            isMagnetized = true;

            // Player 전체 찾음
            targetPlayer = other.transform.root;
        }
    }

    void EatGem()
    {
        // [추가] 소리 재생 (보석이 사라져도 소리는 끝까지 남음)
        if (pickupSound != null) AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);

        PlayerLevel playerLevel = targetPlayer.GetComponent<PlayerLevel>();
        if (playerLevel != null) playerLevel.GainExp(expAmount);

        Destroy(gameObject);
    }
}