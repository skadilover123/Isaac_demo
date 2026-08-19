using UnityEngine;

public class Bullet : MonoBehaviour
{

    [Header("飞行参数")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifeTime = 2f;

    [Header("伤害")]
    [SerializeField] private int bulletDamage = 5;

    private Rigidbody2D rb;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Fire(Vector2 direction)
    {
        Vector2 dir = direction.normalized;
        rb.velocity = dir * bulletSpeed;
        Destroy(gameObject, bulletLifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(bulletDamage);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
