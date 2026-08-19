using UnityEngine;

[DefaultExecutionOrder(200)]
public class Knockback : MonoBehaviour
{

    [Header("击退参数")]
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float strength = 6f;

    private Rigidbody2D rb;
    private Vector2 kbVelocity;
    private float kbTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Apply(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        kbVelocity = direction.normalized * strength;
        kbTimer = duration;
    }

    private void FixedUpdate()
    {
        if (kbTimer <= 0f) return;

        rb.velocity = kbVelocity;
        kbVelocity = kbVelocity * 0.9f;
        kbTimer = kbTimer - Time.fixedDeltaTime;
    }
}
