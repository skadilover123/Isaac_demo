using UnityEngine;

/// <summary>
/// 敌人生成器：挂在场景中的空物体上。
/// 游戏开始时在生成范围内随机位置、随机类型地生成一批敌人，
/// 之后可每隔一段时间补充一个（直到场上存活达到上限）。
/// 使用时把敌人预制体拖进 enemyPrefabs 列表，把玩家拖给 player 即可。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ===================== 敌人预制体 =====================
    [Header("敌人预制体")]
    [SerializeField] private Enemy[] enemyPrefabs;   // 所有可生成的敌人类型（预制体，可放多个）
    [SerializeField] private Transform player;       // 玩家（作为敌人的追击目标，不拖会自动查找）

    // ===================== 开局生成 =====================
    [Header("开局生成")]
    [SerializeField] private int minCount = 4;                     // 开局最少生成数量
    [SerializeField] private int maxCount = 8;                     // 开局最多生成数量
    [SerializeField] private Vector2 spawnArea = new Vector2(20f, 16f);  // 生成范围（以本物体为中心，宽高）

    // ===================== 持续补充 =====================
    [Header("持续补充")]
    [SerializeField] private float respawnInterval = 3f;  // 每隔几秒补充一个敌人；设为 0 表示只开局生成一次
    [SerializeField] private int maxAlive = 10;           // 场上同时存活的最大数量（达到后不再生成）
    [SerializeField] private int totalEnemies = 15;       // 本局总怪物数（含未生成的，杀满这个数即胜利）

    // ===================== 生成安全距离 =====================
    [Header("生成安全距离")]
    [SerializeField] private float minPlayerDist = 3f;    // 与玩家的最小生成距离（避免敌人直接刷在玩家脸上）

    private float respawnTimer;   // 补充计时器
    private int spawnCount;       // 累计已生成的怪物数

    private void Start()
    {
        // 先向管理器注册本局总目标数（含未生成的，避免提前判定胜利）
        if (GameManager.Instance != null) GameManager.Instance.AddTotalEnemies(totalEnemies);

        // 开局：随机数量生成一批敌人（不超过总目标数）
        int count = Random.Range(minCount, maxCount + 1);
        if (count > totalEnemies) count = totalEnemies;
        for (int i = 0; i < count; i++) SpawnOne();

        respawnTimer = respawnInterval;
    }

    private void Update()
    {
        // 不补充的情况：直接返回
        if (respawnInterval <= 0f) return;

        respawnTimer -= Time.deltaTime;
        if (respawnTimer > 0f) return;

        respawnTimer = respawnInterval;

        // 总目标已生成完，或存活敌人达到上限 → 不再补充
        if (spawnCount >= totalEnemies) return;
        int alive = FindObjectsOfType<Enemy>().Length;
        if (alive < maxAlive) SpawnOne();
    }

    /// <summary>随机选一种敌人，在生成范围内随机位置生成，并把玩家设为它的追击目标</summary>
    private void SpawnOne()
    {
        // 总目标已生成完
        if (spawnCount >= totalEnemies) return;

        if (enemyPrefabs.Length == 0)
        {
            Debug.LogError("[EnemySpawner] 请先在 Inspector 中把敌人预制体拖进 enemyPrefabs 列表");
            return;
        }

        // 随机选一种敌人类型
        Enemy prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // 在生成范围内取随机位置（自动避开玩家周围）
        Vector2 pos = RandomPos();

        // 实例化敌人并指定追击目标
        Enemy enemy = Instantiate(prefab, pos, Quaternion.identity);
        enemy.SetTarget(player);
        spawnCount++;
    }

    /// <summary>在生成范围内随机取位置，保证与玩家的距离不小于 minPlayerDist（最多尝试 20 次）</summary>
    private Vector2 RandomPos()
    {
        float halfW = spawnArea.x * 0.5f;
        float halfH = spawnArea.y * 0.5f;

        for (int i = 0; i < 20; i++)
        {
            float x = transform.position.x + Random.Range(-halfW, halfW);
            float y = transform.position.y + Random.Range(-halfH, halfH);
            Vector2 pos = new Vector2(x, y);

            // 没有玩家引用时直接返回（敌人会自动查找玩家）
            if (player == null) return pos;

            // 满足最小距离要求就采用
            float dist = Vector2.Distance(pos, player.position);
            if (dist >= minPlayerDist) return pos;
        }

        // 尝试 20 次仍未满足（生成范围太小等极端情况），退回一个随机位置
        float fx = transform.position.x + Random.Range(-halfW, halfW);
        float fy = transform.position.y + Random.Range(-halfH, halfH);
        return new Vector2(fx, fy);
    }
}
