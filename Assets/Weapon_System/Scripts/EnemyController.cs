using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("기본 능력치")]
    public float maxHealth = 30f;
    public float moveSpeed = 3f;

    [Header("공격 설정")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackRate = 1.5f;

    // [추가됨] 원거리 공격 설정 -------
    [Header("원거리 공격 (박쥐용)")]
    public bool isRanged = false;       // 이거 켜면 원거리 공격함
    public GameObject projectilePrefab; // 발사할 총알
    public Transform firePoint;         // 발사 위치 (입)
    // -----------------------------

    [Header("비행 설정 (박쥐용)")]
    public bool isFlying = false;
    public float flyHeight = 2.0f;
    public float bobSpeed = 2.0f;
    public float bobAmount = 0.5f;

    [Header("사운드")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip attackSound;
    private AudioSource audioSource;

    private float currentHealth;
    private Transform player;
    private PlayerHealth playerHealthScript;
    private Animator anim;
    private bool isDead = false;
    private float nextAttackTime = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        GameObject target = GameObject.FindGameObjectWithTag("Player");
        if (target != null)
        {
            player = target.transform;
            playerHealthScript = target.GetComponent<PlayerHealth>();
        }
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. 공격할 때도 항상 플레이어를 바라봐야 함 (원거리 조준 위해)
        if (distanceToPlayer <= attackRange)
        {
            Vector3 targetPostition = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(targetPostition); // 공격 중에도 플레이어 주시

            AttackBehavior();
        }
        else
        {
            ChaseBehavior();
        }
    }

    void ChaseBehavior()
    {
        if (anim != null)
        {
            anim.SetBool("isMoving", true);
        }

        Vector3 targetPostition = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(targetPostition);

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        if (isFlying)
        {
            float newY = flyHeight + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    void AttackBehavior()
    {
        if (anim != null) anim.SetBool("isMoving", false);

        if (Time.time >= nextAttackTime)
        {
            // 애니메이션 실행
            if (anim != null) anim.SetTrigger("doAttack");

            // [변경됨] 원거리냐 근거리냐에 따라 다르게 공격
            if (isRanged)
            {
                // 원거리 공격: 총알 발사!
                Debug.Log("발사 위치(입): " + firePoint.position);

                if (projectilePrefab != null && firePoint != null)
                {
                    GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

                    // [확인 2] 생성된 총알이 실제로 어디에 생겼는지 확인
                    Debug.Log("생성된 총알 위치: " + bullet.transform.position);
                }
            }
            else
            {
                // 근거리 공격: 즉시 데미지
                if (playerHealthScript != null)
                {
                    playerHealthScript.TakeDamage(attackDamage);
                }
            }

            // 소리 재생
            if (audioSource != null && attackSound != null)
                audioSource.PlayOneShot(attackSound);

            nextAttackTime = Time.time + attackRate;
        }
    }

    // ... TakeDamage, Die 함수는 그대로 유지 ...
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;
        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.useGravity = true; rb.isKinematic = false; }
        if (anim != null) anim.SetTrigger("doDie");
        if (audioSource != null && deathSound != null) audioSource.PlayOneShot(deathSound);
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 3f);
    }
}