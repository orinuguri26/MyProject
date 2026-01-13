using UnityEngine;

public enum EnemyType { Slime, Bat, Ghost }

public class Enemy : MonoBehaviour
{
    [Header("몬스터 타입")]
    public EnemyType enemyType;

    [Header("기본 이동/체력")]
    public float speed = 1.5f;
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("체력바 UI")]
    public GameObject healthBarPrefab;        // 프리팹 연결
    private HealthBarUI healthBarUI;          // 인스턴스된 체력바
    public float healthBarOffsetY = 1.8f;     // 머리 위 위치

    [Header("공격 설정 (Ghost/Slime 전용)")]
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    public float attackRate = 1.5f;
    private float nextAttackTime = 0f;

    [Header("박쥐 원거리 공격")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("박쥐 설정")]
    public float flyHeight = 2.0f;
    public float spawnOffsetY = 0f;

    [Header("사운드 (선택)")]
    public AudioClip attackSound;
    public AudioClip deathSound;
    public AudioClip hitSound;
    private AudioSource audioSource;

    [Header("경험치 구슬 설정")]
    public int dropCount = 1;          // 죽을 때 떨어질 구슬 개수
    public GameObject expGemPrefab;

    [Header("드랍 아이템")]
    public GameObject medkitPrefab;

    [Range(0f, 1f)]
    public float dropChance = 0.3f;

    private Animator anim;
    private Transform player;
    private PlayerHealth playerHealthScript;
    private bool isDead = false;

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

        if (enemyType == EnemyType.Bat)
            flyHeight += spawnOffsetY;

        LookAtPlayer();

        // ------------------------------
        // 체력바 생성
        // ------------------------------
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab);
            healthBarUI = hb.GetComponent<HealthBarUI>();

            // 체력바를 이 몬스터의 자식으로 붙이기
            hb.transform.SetParent(transform);

            // 머리 위에 위치시키기
            hb.transform.localPosition = new Vector3(0, healthBarOffsetY, 0);

            // 초기 체력 설정
            healthBarUI.SetMaxHealth(maxHealth);
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        switch (enemyType)
        {
            case EnemyType.Slime:
                ChasePlayer_Ground();
                break;

            case EnemyType.Bat:
                FlyMovement();
                BatAttack();
                break;

            case EnemyType.Ghost:
                if (distance <= attackRange)
                    GhostAttack();
                else
                    ChasePlayer_Ground();
                break;
        }

        LookAtPlayer();

        // ------------------------------
        // 체력바 Billboard(카메라 바라보기)
        // ------------------------------
        if (healthBarUI != null)
        {
            Transform hb = healthBarUI.transform;
            Transform cam = Camera.main.transform;
            hb.LookAt(hb.position + cam.forward);
        }
    }

    // ---------------- 이동 -----------------
    void ChasePlayer_Ground()
    {
        if (anim != null) anim.SetBool("isMoving", true);

        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    void FlyMovement()
    {
        if (anim != null) anim.SetBool("isMoving", true);

        float bob = Mathf.Sin(Time.time * 2f) * 0.3f;
        transform.position = new Vector3(transform.position.x, flyHeight + bob, transform.position.z);
    }

    // ---------------- Bat 공격 -----------------
    void BatAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackRate;

        if (anim != null) anim.SetTrigger("doAttack");

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        if (projectilePrefab != null && firePoint != null)
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }

    // ---------------- Ghost 공격 -----------------
    void GhostAttack()
    {
        if (anim != null) anim.SetBool("isMoving", false);
        if (Time.time < nextAttackTime) return;

        if (anim != null) anim.SetTrigger("doAttack");

        if (audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound);

        if (playerHealthScript != null)
            playerHealthScript.TakeDamage(attackDamage);

        nextAttackTime = Time.time + attackRate;
    }

    // ---------------- Slime 충돌 공격 -----------------
    private void OnCollisionEnter(Collision collision)
    {
        if (enemyType != EnemyType.Slime) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (anim != null) anim.SetTrigger("doAttack");
            if (playerHealthScript != null)
                playerHealthScript.TakeDamage(attackDamage);

            Die();
        }
    }

    // ---------------- 데미지 처리 -----------------
    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;

        if (anim != null) anim.SetTrigger("doDamaged");
        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        // 체력바 업데이트
        if (healthBarUI != null)
            healthBarUI.SetHealth(currentHealth / maxHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger("doDie");

        DropMedkit();

        if (expGemPrefab != null)
        {
            for (int i = 0; i < dropCount; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 0.5f;
                randomOffset.y = 0;

                Vector3 spawnPos = transform.position + randomOffset;
                spawnPos.y = 0.5f;

                Instantiate(expGemPrefab, spawnPos, Quaternion.identity);
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (enemyType == EnemyType.Bat)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            else
            {
                rb.isKinematic = true; // 충돌 제거
                rb.useGravity = false; // 튀는 거 방지
            }
        }

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 체력바 제거
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);

        Destroy(gameObject, 1.5f);
    }

    // ---------------- LookAt -----------------
    void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 lookPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookPos);
    }
    void DropMedkit()
    {
        if (medkitPrefab == null) return;

        // 드랍 확률 체크
        if (Random.value > dropChance) return;

        Vector3 spawnPos = new Vector3(transform.position.x, 1f, transform.position.z);
        Instantiate(medkitPrefab, spawnPos, Quaternion.identity);
    }
}
