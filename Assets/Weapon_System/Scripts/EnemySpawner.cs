using UnityEngine;

//public class EnemySpawner : MonoBehaviour
//{
//    [Header("스폰 설정")]
//    // [변경점 1] GameObject 하나가 아니라 '배열([])'로 바꿈!
//    public GameObject[] enemyPrefabs;

//    public float spawnInterval = 2f;
//    public float spawnRadius = 15f;

//    private Transform player;
//    private float timer = 0f;

//    void Start()
//    {
//        GameObject p = GameObject.FindGameObjectWithTag("Player");
//        if (p != null) player = p.transform;
//    }

//    void Update()
//    {
//        if (player == null) return;

//        timer += Time.deltaTime;

//        if (timer >= spawnInterval)
//        {
//            SpawnZombie();
//            timer = 0f;
//        }
//    }

//    void SpawnZombie()
//    {
//        // 1. 랜덤 위치 계산
//        Vector2 randomPoint = Random.insideUnitCircle.normalized * spawnRadius;
//        Vector3 spawnPos = new Vector3(randomPoint.x, 0, randomPoint.y) + player.position;
//        spawnPos.y = 1f;

//        // [변경점 2] 목록에서 랜덤으로 하나 뽑기
//        // 0번부터 목록 개수(Length) 사이의 랜덤 숫자 뽑기
//        int randomIndex = Random.Range(0, enemyPrefabs.Length);

//        // [변경점 3] 뽑힌 녀석(randomIndex)을 소환
//        Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);
//    }
//}