using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("총알 설정")]
    public float speed = 20f;       // 속도
    public float damage = 10f;      // 데미지
    public float lifeTime = 3f;     // 수명

    [Header("관통 설정")]
    public int penetration = 0;

    [Header("이펙트 설정")]
    // [추가됨] 맞았을 때 터질 이펙트(피) 프리팹
    public GameObject hitEffectPrefab;

    void Start()
    {
        Destroy(gameObject, lifeTime); // 시간 지나면 삭제
    }

    void Update()
    {
        // 앞으로 날아가기 (Space.Self 기준)
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. 적(Enemy)과 부딪혔을 때
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage); // 데미지 주기
            }

            if (hitEffectPrefab != null)
            {
                GameObject hitVFX = Instantiate(hitEffectPrefab, transform.position, transform.rotation);

                Destroy(hitVFX, 1f);
            }

            // [핵심] 관통 로직
            if (penetration > 0)
            {
                // 관통력이 남아있다면?
                penetration--; // 횟수 1 차감
                // 여기서 Destroy(gameObject)를 호출하지 않으므로 총알은 계속 날아갑니다 (관통)
            }
            else
            {
                Destroy(gameObject); // 관통 횟수 없으면 삭제
            }
        }
        // 2. 벽이나 땅에 부딪혔을 때 (여기는 무조건 삭제)
        else if (other.CompareTag("Wall") || other.CompareTag("Object"))
        {
            Destroy(gameObject);
        }
    }
}