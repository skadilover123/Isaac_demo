using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Tooltip("摄像机跟随的平滑系数（0~1，越大越快）")]
    private float smoothFactor = 0.5f;
    [SerializeField, Tooltip("摄像机与目标的偏移量（相对位置）")]
    private Vector3 offset;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothFactor*Time.deltaTime*60);
    }
}
