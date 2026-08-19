using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [Header("敌人预制体")]
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Transform player;

    [Header("开局生成")]
    [SerializeField] private int minCount = 4;
    [SerializeField] private int maxCount = 8;
    [SerializeField] private Vector2 spawnArea = new Vector2(20f, 16f);

    [Header("持续补充")]
    [SerializeField] private float respawnInterval = 3f;
    [SerializeField] private int maxAlive = 10;
    [SerializeField] private int totalEnemies = 15;

    [Header("生成安全距离")]
    [SerializeField] private float minPlayerDist = 10f;

    private float respawnTimer;
    private int spawnCount;

    private void Start()
    {

        if (GameManager.Instance != null) GameManager.Instance.AddTotalEnemies(totalEnemies);

        int count = Random.Range(minCount, maxCount + 1);
        if (count > totalEnemies) count = totalEnemies;
        for (int i = 0; i < count; i++) SpawnOne();

        respawnTimer = respawnInterval;
    }

    private void Update()
    {

        if (respawnInterval <= 0f) return;

        respawnTimer -= Time.deltaTime;
        if (respawnTimer > 0f) return;

        respawnTimer = respawnInterval;

        if (spawnCount >= totalEnemies) return;
        int alive = FindObjectsOfType<Enemy>().Length;
        if (alive < maxAlive) SpawnOne();
    }

    private void SpawnOne()
    {

        if (spawnCount >= totalEnemies) return;
        Enemy prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Vector2 pos = RandomPos();

        Enemy enemy = Instantiate(prefab, pos, Quaternion.identity);
        enemy.SetTarget(player);
        spawnCount++;
    }

    private Vector2 RandomPos()
    {
        float halfW = spawnArea.x * 0.5f;
        float halfH = spawnArea.y * 0.5f;

        for (int i = 0; i < 20; i++)
        {
            float x = transform.position.x + Random.Range(-halfW, halfW);
            float y = transform.position.y + Random.Range(-halfH, halfH);
            Vector2 pos = new Vector2(x, y);

            if (player == null) return pos;

            float dist = Vector2.Distance(pos, player.position);
            if (dist >= minPlayerDist) return pos;
        }

        float fx = transform.position.x + Random.Range(-halfW, halfW);
        float fy = transform.position.y + Random.Range(-halfH, halfH);
        return new Vector2(fx, fy);
    }
}
