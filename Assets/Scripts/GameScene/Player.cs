using UnityEngine;

/// <summary>
/// 玩家类：整合了移动、射击、血量、受击检测四大模块，挂在玩家根物体上。
/// 玩家结构约定：
///   - 根物体：刚体、碰撞体、本组件（移动/射击/血量逻辑）、击退组件、两个音频源
///   - Head 子物体：射击动画（攻击时显示）
///   - Body 子物体：移动动画，左右转向时整体翻转
///   - Action 子物体：受伤/死亡动画
/// 外部协作：子弹由本类发射；敌人的 Trigger 碰撞体与玩家碰撞体重叠时，
/// 通过 OnTriggerStay2D 在这里处理玩家受击。
/// </summary>
public class Player : MonoBehaviour
{
    // ===================== 移动参数 =====================
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;       // 移动速度
    [SerializeField] private Rigidbody2D rb;             // 玩家刚体（物理移动用）
    [SerializeField] private Transform bodyTransform;    // 身体子物体（左右转向时翻转）

    // ===================== 射击参数 =====================
    [Header("射击参数")]
    [SerializeField] private GameObject bulletPrefab;    // 子弹预制体
    [SerializeField] private float fireInterval = 0.2f;  // 连续射击间隔（秒）
    [SerializeField] private bool eightWay;              // 勾选支持八方向射击，不勾为四方向

    // ===================== 血量参数 =====================
    [Header("血量参数")]
    [SerializeField] private int maxHp = 6;              // 最大生命值
    [SerializeField] private float invulnTime = 1f;      // 受伤后的无敌时间（秒）
    [SerializeField] private float hurtAnimLength = 0.5f; // 受伤动画播放时长（秒）

    // ===================== 外观引用 =====================
    [Header("外观引用")]
    [SerializeField] private Transform head;             // 头部子物体（正常/射击时显示）
    [SerializeField] private Transform body;             // 身体子物体（正常时显示）
    [SerializeField] private Transform action;           // 动作子物体（受伤/死亡时显示）
    [SerializeField] private Animator hurtAnim;          // 受伤动画（action 上的 Animator）
    [SerializeField] private Animator deadAnim;          // 死亡动画（action 上的 Animator）
    [SerializeField] private Collider2D playerCollider;  // 玩家碰撞体

    // ===================== 动画引用 =====================
    [Header("动画引用")]
    [SerializeField] private Animator moveAnim;          // 身体移动动画（Body 上的 Animator）
    [SerializeField] private Animator attackAnim;        // 头部射击动画（Head 上的 Animator）

    // ===================== 运行时状态 =====================
    private Vector2 moveInput;                  // 本帧移动输入
    private int facing = 1;                     // 当前朝向：1 朝右，-1 朝左
    private Vector2 fireDirection = Vector2.up; // 当前射击方向
    private bool isFiring;                      // 是否正在射击
    private float fireTimer;                    // 射击间隔计时器
    private int hp;                             // 当前生命值
    private float invulnTimer;                  // 无敌计时器
    private float hurtTimer;                    // 受伤动画计时器
    private bool isDead;                        // 是否已死亡
    private bool isGameOver;                    // 游戏是否已结束（胜利/失败后由 GameManager 设置）
    private AudioSource hurtAudio;              // 受伤音效（第 1 个 AudioSource）
    private AudioSource deathAudio;             // 死亡音效（第 2 个 AudioSource）

    // 对外只读属性（供 GameManager、敌人查询）
    public int Hp { get { return hp; } }
    public int MaxHp { get { return maxHp; } }
    public bool IsDead { get { return isDead; } }
    public bool IsInvulnerable { get { return invulnTimer > 0f; } }

    /// <summary>游戏结束（胜利/失败）时由 GameManager 调用：停止玩家所有行动与动画</summary>
    public void StopControl()
    {
        isGameOver = true;

        // 同时清掉速度，避免停住时还带着惯性
        if (rb != null) rb.velocity = Vector2.zero;

        // 复位移动动画参数，让角色停在待机姿势（不再继续播放移动动画）
        if (moveAnim != null)
        {
            moveAnim.SetBool(IsMovingLR, false);
            moveAnim.SetBool(IsMovingU, false);
            moveAnim.SetBool(IsMovingD, false);
        }

        // 复位射击动画参数，停止射击动画
        if (attackAnim != null)
        {
            attackAnim.SetBool("ShotUp", false);
            attackAnim.SetBool("ShotDown", false);
            attackAnim.SetBool("ShotLeft", false);
            attackAnim.SetBool("ShotRight", false);
        }
    }

    // 动画参数哈希（避免每帧做字符串查找）
    private static readonly int IsMovingLR = Animator.StringToHash("isMovingleft_right");
    private static readonly int IsMovingD = Animator.StringToHash("isMovingdown");
    private static readonly int IsMovingU = Animator.StringToHash("isMovingup");
    private static readonly int IsHurt = Animator.StringToHash("isHurt");
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");

    private void Awake()
    {
        hp = maxHp;

        // 音效：按顺序取第 1、2 个 AudioSource，分别用作受伤与死亡音效
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length > 0) hurtAudio = audioSources[0];
        if (audioSources.Length > 1) deathAudio = audioSources[1];

        // 组件引用兜底
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
        if (attackAnim == null) attackAnim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // 死亡或游戏结束后不再处理任何输入与逻辑（移动与射击全部停止）
        if (isDead || isGameOver) return;

        ReadMoveInput();
        HandleShootInput();
        UpdateShootAnim();

        // 无敌与受伤动画计时
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        if (hurtTimer > 0f)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0f) EndHurt();
        }

        // 持续射击：按间隔定时发射
        if (isFiring)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                Fire();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isGameOver) return;

        // 物理移动：对角线输入归一化，避免斜向时速度变大
        if (moveInput.sqrMagnitude > 0f)
            rb.velocity = moveInput.normalized * moveSpeed;
        else
            rb.velocity = Vector2.zero;
    }

    // ===================== 移动逻辑 =====================

    /// <summary>读取 WASD 输入，处理转向并驱动移动动画</summary>
    private void ReadMoveInput()
    {
        // 采集输入：四个方向相互独立，可同时按下形成斜向
        float x = 0f;
        float y = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.W)) y += 1f;
        if (Input.GetKey(KeyCode.S)) y -= 1f;
        moveInput = new Vector2(x, y);

        // 转向：只有左右输入才改变朝向，仅在方向真正变化时写 localScale
        if (x != 0f)
        {
            int newFacing;
            if (x > 0f) newFacing = 1;
            else newFacing = -1;

            if (newFacing != facing)
            {
                facing = newFacing;
                bodyTransform.localScale = new Vector3(facing, 1f, 1f) * 2f;
            }
        }

        // 移动动画优先级：左右 > 上下；静止时全部关闭
        bool hasHorizontal = x != 0f;
        bool hasVertical = y != 0f;
        if (moveAnim != null)
        {
            moveAnim.SetBool(IsMovingLR, hasHorizontal);
            moveAnim.SetBool(IsMovingU, !hasHorizontal && hasVertical && y > 0f);
            moveAnim.SetBool(IsMovingD, !hasHorizontal && hasVertical && y < 0f);
        }
    }

    // ===================== 射击逻辑 =====================

    /// <summary>读取方向键输入，决定是否射击以及射击方向</summary>
    private void HandleShootInput()
    {
        // 按 Ctrl 键取消射击
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isFiring = false;
            return;
        }

        bool up = Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);

        float x = 0f;
        float y = 0f;
        if (right) x += 1f;
        if (left) x -= 1f;
        if (up) y += 1f;
        if (down) y -= 1f;
        Vector2 input = new Vector2(x, y);

        // 按下任意方向键才开始射击
        if (!isFiring)
        {
            bool pressed = Input.GetKeyDown(KeyCode.UpArrow) ||
                           Input.GetKeyDown(KeyCode.DownArrow) ||
                           Input.GetKeyDown(KeyCode.LeftArrow) ||
                           Input.GetKeyDown(KeyCode.RightArrow);
            if (pressed) isFiring = true;
            else return;
        }

        // 更新射击方向：八向直接归一化，四向吸附到主轴
        if (input != Vector2.zero)
        {
            if (eightWay) fireDirection = input.normalized;
            else fireDirection = Snap4(input);
        }
    }

    /// <summary>把任意方向吸附到最近的上下左右四个主轴方向</summary>
    private static Vector2 Snap4(Vector2 v)
    {
        float absX = Mathf.Abs(v.x);
        float absY = Mathf.Abs(v.y);
        if (absX >= absY)
        {
            if (v.x != 0f) return new Vector2(Mathf.Sign(v.x), 0f);
            return new Vector2(0f, Mathf.Sign(v.y));
        }
        return new Vector2(0f, Mathf.Sign(v.y));
    }

    /// <summary>在当前位置生成一颗子弹并朝射击方向发射</summary>
    private void Fire()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.Fire(fireDirection);
    }

    /// <summary>根据射击方向切换头部射击动画（上下左右四向）</summary>
    private void UpdateShootAnim()
    {
        if (attackAnim == null) return;

        Vector2 dir = AnimDirection(fireDirection);
        attackAnim.SetBool("ShotUp", isFiring && dir == Vector2.up);
        attackAnim.SetBool("ShotDown", isFiring && dir == Vector2.down);
        attackAnim.SetBool("ShotLeft", isFiring && dir == Vector2.left);
        attackAnim.SetBool("ShotRight", isFiring && dir == Vector2.right);
    }

    /// <summary>把射击方向映射到四向动画方向</summary>
    private static Vector2 AnimDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return Vector2.zero;

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);
        if (absX >= absY)
        {
            if (dir.x >= 0f) return Vector2.right;
            return Vector2.left;
        }
        if (dir.y >= 0f) return Vector2.up;
        return Vector2.down;
    }

    // ===================== 受击与血量 =====================

    /// <summary>受到伤害：无敌时间内免疫，血量归零则死亡</summary>
    public void TakeDamage(int amount)
    {
        if (isDead || invulnTimer > 0f) return;

        hp -= amount;
        if (hp > 0)
        {
            if (hurtAudio != null) hurtAudio.Play();
            invulnTimer = invulnTime;
            StartHurt();
        }
        else
        {
            hp = 0;
            Die();
        }
    }

    /// <summary>
    /// 受击检测：敌人的 Trigger 碰撞体与玩家碰撞体重叠时回调。
    /// 命中后对玩家施加击退，同时让敌人轻微后退。
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;
        if (isDead || invulnTimer > 0f) return;

        TakeDamage(enemy.Damage);

        Vector2 dir = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;
        Knockback playerKb = GetComponent<Knockback>();
        Knockback enemyKb = other.GetComponentInParent<Knockback>();
        if (playerKb != null) playerKb.Apply(dir);
        if (enemyKb != null) enemyKb.Apply(-dir);
    }

    /// <summary>开始播放受伤动画：隐藏头/身，显示动作层</summary>
    private void StartHurt()
    {
        if (hurtAnim == null && action != null) hurtAnim = action.GetComponent<Animator>();
        if (hurtAnim != null) hurtAnim.SetBool(IsHurt, true);
        if (head != null) head.gameObject.SetActive(false);
        if (body != null) body.gameObject.SetActive(false);
        if (action != null) action.gameObject.SetActive(true);
        hurtTimer = hurtAnimLength;
    }

    /// <summary>受伤动画播完：恢复头/身显示，隐藏动作层</summary>
    private void EndHurt()
    {
        if (hurtAnim != null) hurtAnim.SetBool(IsHurt, false);
        if (head != null) head.gameObject.SetActive(true);
        if (body != null) body.gameObject.SetActive(true);
        if (action != null) action.gameObject.SetActive(false);
    }

    /// <summary>玩家死亡：停止逻辑与击退、切换外观、播放死亡动画与音效</summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        hurtTimer = 0f;

        // 停止物理与击退，避免死亡瞬间被弹飞
        if (rb != null) rb.velocity = Vector2.zero;
        Knockback kb = GetComponent<Knockback>();
        if (kb != null) kb.enabled = false;

        // 隐藏头/身，显示整体动作层
        if (head != null) head.gameObject.SetActive(false);
        if (body != null) body.gameObject.SetActive(false);
        if (action != null) action.gameObject.SetActive(true);

        // 触发死亡动画（未拖引用时自动从 action 上取）
        if (deadAnim == null && action != null) deadAnim = action.GetComponent<Animator>();
        if (deadAnim != null)
        {
            deadAnim.SetBool(IsHurt, false);
            deadAnim.SetBool(IsDeadHash, true);
        }
        else
        {
            Debug.LogError("[Player.Die] 死亡动画 Animator 未赋值，请把 action 上的 Animator 拖给 deadAnim");
        }

        if (deathAudio != null) deathAudio.Play();
    }
}
