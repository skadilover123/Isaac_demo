using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform target;

    [Header("跟随参数")]
    [SerializeField] private float smoothFactor = 0.5f;
    [SerializeField] private Vector3 offset;

    private void LateUpdate()
    {

        if (target == null) return;

        Vector3 goal = target.position + offset;
        float lerpFactor = smoothFactor * Time.deltaTime * 60f;
        transform.position = Vector3.Lerp(transform.position, goal, lerpFactor);
    }
}
