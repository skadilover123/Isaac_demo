using UnityEngine;

/// <summary>
/// 敌人基类：后续所有敌人继承此类，挂在敌人根物体上。
/// 整合了直线追踪移动、血量、伤害与死亡逻辑（原 EnemyMove + EnemyHealth + EnemyDamage）。
/// 敌人结构约定：
///   - 根物体：刚体、Trigger 碰撞体、本组件（移动/血量/伤害）、击退组件、动画、精灵
///   - 碰撞体设为 Trigger，与玩家碰撞体重叠时由玩家侧处理受击
/// 子类通过覆写虚方法定制行为：
///   Move()       定制移动方式（飞行、冲锋、静止炮台等）
///   OnHit()      受击反馈（闪白、飘字、音效等）
///   OnDie()      死亡反馈（掉落物、粒子、死亡音效等）
/// </summary>
public class Enemy : MonoBehaviour
{
    // ===================== 移动参数 =====================
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 3f;       // 追击玩家速度
    [SerializeField] private float stopDistance = 0.5f;  // 停止距离（进入后停下，攻击逻辑由子类实现）
    [SerializeField] protected Transform target;         // 追击目标（玩家，Inspector 可拖，留空自动查找）

    // ===================== 属性 =====================
    [Header("属性")]
    [SerializeField] private int maxHp = 10;             // 最大生命值
    [SerializeField] private int damage = 1;             // 触碰玩家时造成的伤害

    // ===================== 组件引用 =====================
    [Header("组件引用")]
    [SerializeField] protected Animator anim;            // 动画控制器
    [SerializeField] protected Rigidbody2D rb;           // 刚体（物理移动用）

    // 对外只读属性（供子弹、玩家受击检测、GameManager 查询）
    public int Hp { get { return currentHp; } }
    public int MaxHp { get { return maxHp; } }
    public int Damage { get { return damage; } }
    public bool IsDead { get { return isDead; } }

    // 运行时状态
    private int currentHp;   // 当前生命值
    private bool isDead;     // 是否已死亡
    private Player player;   // 缓存玩家组件（用于判断玩家是否死亡）

    // 动画参数哈希（避免每帧做字符串查找）
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    protected virtual void Awake()
    {
        currentHp = maxHp;

        // 组件引用兜底：未拖引用时自动获取
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // 目标兜底：未拖引用时自动查找场景中的玩家
        if (target == null)
        {
            Player found = FindObjectOfType<Player>();
            if (found != null) target = found.transform;
        }
        if (target != null) player = target.GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    // ===================== 移动逻辑 =====================

    /// <summary>设置追击目标（生成器生成敌人后调用；预制体内的 target 引用不了场景对象，必须由生成器指定）</summary>
    public void SetTarget(Transform newTarget)
    {
        // 目标为空时不覆盖（Awake 里已自动查找过玩家）
        if (newTarget == null) return;

        target = newTarget;
        player = target.GetComponent<Player>();
    }

    /// <summary>默认直线追向玩家；子类覆写可实现飞行、冲锋、静止炮台等</summary>
    protected virtual void Move()
    {
        // 玩家死亡或目标丢失 → 停下
        if (player != null && player.IsDead)
        {
            SetMoving(false);
            return;
        }
        if (target == null)
        {
            SetMoving(false);
            return;
        }

        // 指向玩家的最短直线方向
        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float dist = toTarget.magnitude;

        // 进入停止距离 → 停下
        if (dist <= stopDistance)
        {
            SetMoving(false);
            return;
        }

        Vector2 dir = toTarget / dist;   // 归一化方向（复用已算出的 dist，避免重复开方）
        rb.velocity = dir * moveSpeed;
        SetMoving(true);
    }

    /// <summary>统一控制速度与移动动画：停下时清零速度并关闭动画</summary>
    private void SetMoving(bool moving)
    {
        if (!moving && rb != null) rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMovingHash, moving);
    }

    // ===================== 受击与死亡 =====================

    /// <summary>受到伤害（由子弹调用），血量归零则死亡</summary>
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHp -= amount;
        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
        else
        {
            OnHit(amount);
        }
    }

    /// <summary>受击钩子：子类覆写（闪白、飘字、音效）</summary>
    protected virtual void OnHit(int amount) { }

    /// <summary>死亡处理：停止移动、通知管理器、触发死亡钩子并销毁</summary>
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null) rb.velocity = Vector2.zero;
        SetMoving(false);

        if (GameManager.Instance != null) GameManager.Instance.OnEnemyKilled();
        OnDie();
        Destroy(gameObject);
    }

    /// <summary>死亡钩子：子类覆写（掉落物、粒子、死亡音效）</summary>
    protected virtual void OnDie() { }

#if UNITY_EDITOR
    /// <summary>场景中选中敌人时绘制停止距离圈，方便调试</summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
#endif
}
