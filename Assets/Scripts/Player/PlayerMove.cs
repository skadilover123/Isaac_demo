using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;


    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform bodyTransform;

    private static readonly int IsMovingLR = Animator.StringToHash("isMovingleft_right");
    private static readonly int IsMovingD = Animator.StringToHash("isMovingdown");
    private static readonly int IsMovingU = Animator.StringToHash("isMovingup");

    // 当前朝向：1 = 朝右，-1 = 朝左
    private int facing = 1;

    // 本帧输入方向：在 Update 读取，在 FixedUpdate 施加到刚体
    private Vector2 moveInput;

    private void Update()
    {
        // 1) 采集 WASD 输入（四个方向相互独立，可同时按下形成斜向）
        float x = 0f, y = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.W)) y += 1f;
        if (Input.GetKey(KeyCode.S)) y -= 1f;
        moveInput = new Vector2(x, y);

        // 2) 转向优化：只有左右输入才改变朝向，且仅在方向真正变化时才写 localScale
        if (x != 0f)
        {
            int newFacing = x > 0f ? 1 : -1;
            if (newFacing != facing)
            {
                facing = newFacing;
                bodyTransform.localScale = 2 * new Vector3(facing, 1f, 1f);
            }
        }

        // 3) 动画优先级：左右 > 上下。
        //    有左右输入 → 播左右动画；只有上下输入（无左右）→ 播上下动画；静止 → 都不播。
        bool hasHorizontal = x != 0f;
        bool hasVertical = y != 0f;
        anim.SetBool(IsMovingLR, hasHorizontal);
        anim.SetBool(IsMovingU, !hasHorizontal && hasVertical && y > 0f);
        anim.SetBool(IsMovingD, !hasHorizontal && hasVertical && y < 0f);
    }

    private void FixedUpdate()
    {
        // 4) 物理移动：在 FixedUpdate 施加速度，保证物理稳定；
        //    对角线归一化，避免斜向时速度 = √2 倍导致更快。
        if (moveInput.sqrMagnitude > 0f)
            rb.velocity = moveInput.normalized * moveSpeed;
        else
            rb.velocity = Vector2.zero;
    }
}
