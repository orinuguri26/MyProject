using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 필요

public class Medkit : MonoBehaviour
{
    public float moveSpeed = 8f;        // Magnet에 빨려오는 속도

    [Header("Pickup Sound")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float volume = 0.5f;

    [Header("Rotation")]
    public float rotateSpeed = 90f;    // 회전 속도 (도/초)

    private Transform targetPlayer;
    private bool isMagnetized = false;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);

        if (isMagnetized && targetPlayer != null)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPlayer.position,
                moveSpeed * Time.deltaTime
            );

            // 일정 거리 이내면 즉시 획득
            if (Vector3.Distance(transform.position, targetPlayer.position) < 0.5f)
            {
                EatMedkit(targetPlayer);
            }
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Magnet"))
        {
            targetPlayer = other.transform.root;  // 전체 Player 객체
            StartCoroutine(StartMagnetAfterDelay(1f)); // 1초 딜레이 후 당겨오기
        }
    }

    private IEnumerator StartMagnetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isMagnetized = true;
    }

    void EatMedkit(Transform player)
    {
        // 사운드 재생
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, volume);

        // 플레이어 인벤토리에 추가
        MedkitInventory inv = player.GetComponent<MedkitInventory>();
        if (inv != null)
        {
            inv.AddMedkit(1);
        }

        // 오브젝트 제거
        Destroy(gameObject);
    }
}
