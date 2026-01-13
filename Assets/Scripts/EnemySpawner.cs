using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[System.Serializable]
public class Wave
{
    public string waveName;
    public float startTime;
    public List<GameObject> enemyPrefabs;
    public AudioClip waveBGM;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("시간 및 승리 설정")]
    public float timeLimit = 10f;
    private float gameTimer = 0f;
    public bool isBossBattle = false;
    private bool isGameClear = false;

    [Header("스폰 설정")]
    public Transform player;
    public float minRadius = 3f;
    public float spawnRadius = 15f;
    public float startSpawnInterval = 2f;
    public float minSpawnInterval = 0.2f;

    [Header("난이도 스케일링")]
    public float difficultyScaling = 1.0f;
    public float speedScaling = 0.5f;

    [Header("웨이브 & 보스 데이터")]
    public List<Wave> waves;
    public GameObject bossPrefab;
    public float bossSpawnOffset = 5.0f;
    private GameObject bossInstance;

    [Header("사운드 설정")]
    public AudioClip bossBGM;
    private AudioSource bgmSource;
    private int currentWaveIndex = -1;

    [Header("UI 연결")]
    public TextMeshProUGUI timerText;
    public GameObject winPanel;

    [Header("VR Input")]
    public InputActionProperty leftTriggerAction; 
    private bool triggerWasPressed = false;     

    [Header("맵 스폰 영역")]
    public Vector3 mapCenter = Vector3.zero;
    public float mapHalfSize = 10f;

    private float nextSpawnTime = 0f;

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);

        bgmSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        gameTimer = 0f;
        isBossBattle = false;
        isGameClear = false;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (isGameClear)
        {
            // 🚨 왼손 트리거로 리스타트
            bool isPressed = leftTriggerAction.action.IsPressed();
            if (isPressed && !triggerWasPressed)
            {
                RestartGame();
            }
            triggerWasPressed = isPressed;

            return;
        }

        if (isBossBattle) CheckBossStatus();

        gameTimer += Time.deltaTime;
        UpdateTimerUI();

        CheckWaveBGM();
        HandleSpawning();

        if (!isBossBattle && gameTimer >= timeLimit)
        {
            Debug.Log("조건 만족, 보스 소환 시도");
            SpawnBoss();
        }
    }

    void CheckWaveBGM()
    {
        int latestActiveIndex = -1;
        for (int i = 0; i < waves.Count; i++)
            if (gameTimer >= waves[i].startTime)
                latestActiveIndex = i;

        if (latestActiveIndex > currentWaveIndex)
        {
            currentWaveIndex = latestActiveIndex;
            if (waves[currentWaveIndex].waveBGM != null)
                PlayBGM(waves[currentWaveIndex].waveBGM);
        }
    }

    void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        if (isBossBattle) return;

        int minutes = Mathf.FloorToInt(gameTimer / 60F);
        int seconds = Mathf.FloorToInt(gameTimer % 60F);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void HandleSpawning()
    {
        if (Time.time >= nextSpawnTime)
        {
            foreach (var wave in waves)
                if (gameTimer >= wave.startTime)
                    SpawnEnemy(wave.enemyPrefabs);

            float progress = gameTimer / timeLimit;
            float currentInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, progress);
            nextSpawnTime = Time.time + currentInterval;
        }
    }

    void SpawnEnemy(List<GameObject> prefabs)
    {
        if (prefabs == null || prefabs.Count == 0) return;
        if (player == null) return;

        int randomIndex = Random.Range(0, prefabs.Count);
        GameObject enemyToSpawn = prefabs[randomIndex];

        EnemySettings setting = enemyToSpawn.GetComponentInChildren<EnemySettings>();
        float offset = (setting != null) ? setting.heightOffset : 1f;

        Vector3 spawnPos;
        int attempts = 0;

        do
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(minRadius, spawnRadius);
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            spawnPos = player.position + new Vector3(x, 0f, z);
            spawnPos.y += offset;

            attempts++;
            if (attempts > 20) return;
        }
        while (!IsInsideMap(spawnPos) || Vector3.Distance(spawnPos, player.position) < minRadius);

        GameObject enemyObj = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);

        EnemyController prefabScript = enemyToSpawn.GetComponent<EnemyController>();
        if (prefabScript != null)
        {
            float progress = gameTimer / timeLimit;
            float statMultiplier = 1f + (progress * difficultyScaling);
            prefabScript.maxHealth *= statMultiplier;
            prefabScript.attackDamage *= statMultiplier;
            float speedMultiplier = 1f + (progress * speedScaling);
            prefabScript.moveSpeed *= speedMultiplier;

            if (prefabScript.isFlying)
                prefabScript.flyHeight = spawnPos.y;
        }
    }

    bool IsInsideMap(Vector3 pos)
    {
        return pos.x >= (mapCenter.x - mapHalfSize) && pos.x <= (mapCenter.x + mapHalfSize) &&
               pos.z >= (mapCenter.z - mapHalfSize) && pos.z <= (mapCenter.z + mapHalfSize);
    }

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("보스 프리팹 없음!");
            return;
        }

        isBossBattle = true;
        if (timerText != null) timerText.text = "<color=red>BOSS BATTLE</color>";

        if (bossBGM != null) PlayBGM(bossBGM);

        if (player != null)
        {
            Vector3 spawnPos;
            int attempts = 0;

            EnemySettings setting = bossPrefab.GetComponentInChildren<EnemySettings>();
            float offset = (setting != null) ? setting.heightOffset : 1f;

            do
            {
                float randomX = Random.Range(mapCenter.x - mapHalfSize, mapCenter.x + mapHalfSize);
                float randomZ = Random.Range(mapCenter.z - mapHalfSize, mapCenter.z + mapHalfSize);
                spawnPos = new Vector3(randomX, player.position.y + offset, randomZ);

                attempts++;
                if (attempts > 50)
                {
                    Debug.LogWarning("보스 스폰 실패");
                    return;
                }

            } while (Vector3.Distance(spawnPos, player.position) < minRadius);

            bossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            bossInstance.transform.LookAt(player);
            Debug.Log("보스 등장!");
        }
    }

    void CheckBossStatus()
    {
        if (isBossBattle && bossInstance == null)
            GameClear();
    }

    void GameClear()
    {
        isGameClear = true;
        if (bgmSource != null) bgmSource.Stop();

        // UI 월드 배치
        Transform cam = Camera.main.transform;
        winPanel.transform.position = cam.position + cam.forward * 0.6f;
        winPanel.transform.rotation = Quaternion.LookRotation(cam.forward, Vector3.up);
        winPanel.transform.position += new Vector3(0f, -0.1f, 0f);
        if (winPanel != null) winPanel.SetActive(true);

        GamePauseManager.RequestPause();

        triggerWasPressed = false;
    }
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
