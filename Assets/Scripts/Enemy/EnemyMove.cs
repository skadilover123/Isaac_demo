using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    [Header("目标")]
    [SerializeField] private Transform target;

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField]
    private float stopDistance = 0.5f;

    [Header("组件引用")]
    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerHealth playerHealth;

    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    private void Awake()
    {
        anim.SetBool(IsMoving, true);
        // 自动从 target 取玩家血量引用（Inspector 拖了也兼容）
        if (target != null) playerHealth = target.GetComponent<PlayerHealth>();
    }

    private void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.IsDead) { 
            Stop(); 
            return; 
        }
        // 目标丢失 → 停下
        if (target == null)
        {
            Stop();
            return;
        }

        // 指向玩家的最短直线方向
        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float dist = toTarget.magnitude;

        // 进入停止距离 → 停下
        if (dist <= stopDistance)
        {
            Stop();
            return;
        }

        Vector2 dir = toTarget / dist; // 归一化方向（复用已算出的 dist，避免重复开方）

        // 物理移动
        rb.velocity = dir * moveSpeed;

    }

    // 停下：清零速度并关闭移动动画
    private void Stop()
    {
        rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMoving, false);
    }
}
