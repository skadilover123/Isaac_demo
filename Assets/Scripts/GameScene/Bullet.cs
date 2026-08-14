using UnityEngine;

/// <summary>
/// 子弹类：由玩家发射，沿指定方向直线飞行。
/// 命中敌人时对其造成伤害，碰到墙壁或存活超时后销毁。
/// </summary>
public class Bullet : MonoBehaviour
{
    // ===================== 飞行参数 =====================
    [Header("飞行参数")]
    [SerializeField] private float bulletSpeed = 10f;     // 飞行速度
    [SerializeField] private float bulletLifeTime = 2f;   // 存活时间（秒），超时自动销毁

    // ===================== 伤害 =====================
    [Header("伤害")]
    [SerializeField] private int bulletDamage = 5;        // 命中敌人时造成的伤害

    private Rigidbody2D rb;   // 刚体（驱动飞行）

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>朝指定方向发射子弹（由 Player 调用），并设置自动销毁计时</summary>
    public void Fire(Vector2 direction)
    {
        Vector2 dir = direction.normalized;
        rb.velocity = dir * bulletSpeed;
        Destroy(gameObject, bulletLifeTime);
    }

    /// <summary>命中敌人造成伤害；命中墙壁销毁；忽略其他物体</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 优先判定敌人：兼容敌人碰撞体挂在子物体上的情况
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(bulletDamage);
            Destroy(gameObject);
            return;
        }

        // 命中墙壁
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
