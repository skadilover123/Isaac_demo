using UnityEngine;

/// <summary>
/// 摄像机跟随：挂在主摄像机上，LateUpdate 中平滑跟随目标并保持固定偏移。
/// 使用 LateUpdate 保证在玩家移动之后计算，画面不会抖动。
/// </summary>
public class CameraMove : MonoBehaviour
{
    // ===================== 跟随目标 =====================
    [Header("跟随目标")]
    [SerializeField] private Transform target;   // 要跟随的物体（玩家）

    // ===================== 跟随参数 =====================
    [Header("跟随参数")]
    [SerializeField] private float smoothFactor = 0.5f;  // 平滑系数（0~1，越大跟随越快）
    [SerializeField] private Vector3 offset;             // 与目标的偏移量（相对位置）

    private void LateUpdate()
    {
        // 目标为空时不移动，避免空引用
        if (target == null) return;

        Vector3 goal = target.position + offset;
        float lerpFactor = smoothFactor * Time.deltaTime * 60f;
        transform.position = Vector3.Lerp(transform.position, goal, lerpFactor);
    }
}
