using UnityEngine;

/// <summary>
/// 击退组件：挂载在玩家或敌人身上，收到击退时短时间内接管刚体速度并逐渐衰减。
/// 执行顺序靠后（DefaultExecutionOrder 200），确保在本帧移动脚本之后执行，
/// 从而覆盖它们写入的 velocity，形成击退效果。
/// </summary>
[DefaultExecutionOrder(200)]
public class Knockback : MonoBehaviour
{
    // ===================== 击退参数 =====================
    [Header("击退参数")]
    [SerializeField] private float duration = 0.2f;   // 击退持续时间（秒）
    [SerializeField] private float strength = 6f;     // 击退初速度

    private Rigidbody2D rb;        // 刚体
    private Vector2 kbVelocity;    // 当前击退速度（随时间衰减）
    private float kbTimer;         // 击退剩余时间

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>施加一次击退：方向为受力方向（通常由受击点指向击退方向）</summary>
    public void Apply(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        kbVelocity = direction.normalized * strength;
        kbTimer = duration;
    }

    private void FixedUpdate()
    {
        if (kbTimer <= 0f) return;

        // 击退窗口内由本组件接管速度，并做平滑衰减
        rb.velocity = kbVelocity;
        kbVelocity = kbVelocity * 0.9f;
        kbTimer = kbTimer - Time.fixedDeltaTime;
    }
}
