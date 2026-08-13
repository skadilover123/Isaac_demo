using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(200)] // 关键：在移动脚本之后执行，从而覆盖它们的 velocity
public class Knockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 kbVelocity;
    private float kbTimer;

    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float strength = 6f;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Apply(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        kbVelocity = direction.normalized * strength;
        kbTimer = duration;
    }

    void FixedUpdate()
    {
        if (kbTimer <= 0) return;
        rb.velocity = kbVelocity;   // 击退窗口内由这里控制速度
        kbVelocity *= 0.9f;         // 平滑衰减
        kbTimer -= Time.fixedDeltaTime;
    }
}
