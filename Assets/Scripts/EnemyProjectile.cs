using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;

    private Vector3 targetDirection; // 발사 순간 목표 방향

    void Start()
    {
        // 발사 순간 플레이어 위치 가져오기
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            Vector3 targetPos = player.position;
            targetDirection = (targetPos - transform.position).normalized;

            // 총알 회전 맞추기
            transform.rotation = Quaternion.LookRotation(targetDirection);
        }

        // 3초 후 삭제
        Destroy(gameObject, 2f);
    }

    void Update()
    {
        // Transform을 직접 이동시켜 직선 이동
        transform.position += targetDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어에게 데미지 처리
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            Debug.Log("히힛 메테오 발싸");
            Destroy(gameObject); // 맞으면 삭제
        }
        else if (other.CompareTag("Object"))
        {
            Destroy(gameObject); // Object와 충돌 시 삭제
        }
    }
}
