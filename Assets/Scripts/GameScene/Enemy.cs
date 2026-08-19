using UnityEngine;

public class Enemy : MonoBehaviour
{

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] protected Transform target;

    [Header("属性")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int damage = 1;

    [Header("组件引用")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected Rigidbody2D rb;

    public int Hp => currentHp;
    public int MaxHp => maxHp;
    public int Damage => damage;
    public bool IsDead => isDead;

    private int currentHp;
    private bool isDead;
    private Player player;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    protected virtual void Awake()
    {
        currentHp = maxHp;

        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (target == null)
        {
            Player found = FindObjectOfType<Player>();
            if (found != null) target = found.transform;
        }
        if (target != null) player = target.GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    public void SetTarget(Transform newTarget)
    {

        if (newTarget == null) return;

        target = newTarget;
        player = target.GetComponent<Player>();
    }

    protected virtual void Move()
    {

        if (player != null && player.IsDead)
        {
            SetMoving(false);
            return;
        }
        if (target == null)
        {
            SetMoving(false);
            return;
        }

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        float dist = toTarget.magnitude;

        if (dist <= stopDistance)
        {
            SetMoving(false);
            return;
        }

        Vector2 dir = toTarget / dist;
        rb.velocity = dir * moveSpeed;
        SetMoving(true);
    }

    private void SetMoving(bool moving)
    {
        if (!moving && rb != null) rb.velocity = Vector2.zero;
        if (anim != null) anim.SetBool(IsMovingHash, moving);
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHp -= amount;
        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
        else
        {
            OnHit(amount);
        }
    }

    protected virtual void OnHit(int amount) { }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (rb != null) rb.velocity = Vector2.zero;
        SetMoving(false);

        if (GameManager.Instance != null) GameManager.Instance.OnEnemyKilled();
        OnDie();
        Destroy(gameObject);
    }

    protected virtual void OnDie() { }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
#endif
}
