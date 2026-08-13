using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifeTime = 2f;
    [SerializeField] private int bulletDamage = 5;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void Fire(Vector2 direction)
    {
        if (rb != null) rb.velocity = direction.normalized * bulletSpeed;
        Destroy(gameObject, bulletLifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null) { enemy.TakeDamage(bulletDamage); Destroy(gameObject); return; }
        if (other.CompareTag("Wall")) Destroy(gameObject);
    }
}
