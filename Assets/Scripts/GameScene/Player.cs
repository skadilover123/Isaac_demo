using UnityEngine;

public class Player : MonoBehaviour
{

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform bodyTransform;

    [Header("射击参数")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField] private bool eightWay;

    [Header("血量参数")]
    [SerializeField] private int maxHp = 6;
    [SerializeField] private float invulnTime = 1f;
    [SerializeField] private float hurtAnimLength = 0.5f;

    [Header("外观引用")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;
    [SerializeField] private Transform action;
    [SerializeField] private Animator hurtAnim;
    [SerializeField] private Animator deadAnim;
    [SerializeField] private Collider2D playerCollider;

    [Header("动画引用")]
    [SerializeField] private Animator moveAnim;
    [SerializeField] private Animator attackAnim;

    private Vector2 moveInput;
    private int facing = 1;
    private Vector2 fireDirection = Vector2.up;
    private bool isFiring;
    private float fireTimer;
    private int hp;
    private float invulnTimer;
    private float hurtTimer;
    private bool isDead;
    private bool isGameOver;
    private AudioSource hurtAudio;
    private AudioSource deathAudio;

    public int Hp { get { return hp; } }
    public int MaxHp { get { return maxHp; } }
    public bool IsDead { get { return isDead; } }
    public bool IsInvulnerable { get { return invulnTimer > 0f; } }

    public void StopControl()
    {
        isGameOver = true;

        if (rb != null) rb.velocity = Vector2.zero;

        if (moveAnim != null)
        {
            moveAnim.SetBool(IsMovingLR, false);
            moveAnim.SetBool(IsMovingU, false);
            moveAnim.SetBool(IsMovingD, false);
        }

        if (attackAnim != null)
        {
            attackAnim.SetBool("ShotUp", false);
            attackAnim.SetBool("ShotDown", false);
            attackAnim.SetBool("ShotLeft", false);
            attackAnim.SetBool("ShotRight", false);
        }
    }

    private static readonly int IsMovingLR = Animator.StringToHash("isMovingleft_right");
    private static readonly int IsMovingD = Animator.StringToHash("isMovingdown");
    private static readonly int IsMovingU = Animator.StringToHash("isMovingup");
    private static readonly int IsHurt = Animator.StringToHash("isHurt");
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");

    private void Awake()
    {
        hp = maxHp;

        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length > 0) hurtAudio = audioSources[0];
        if (audioSources.Length > 1) deathAudio = audioSources[1];

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
        if (attackAnim == null) attackAnim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {

        if (isDead || isGameOver) return;

        ReadMoveInput();
        HandleShootInput();
        UpdateShootAnim();

        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        if (hurtTimer > 0f)
        {
            hurtTimer -= Time.deltaTime;
            if (hurtTimer <= 0f) EndHurt();
        }

        if (isFiring)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval)
            {
                fireTimer = 0f;
                Fire();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isGameOver) return;

        if (moveInput.sqrMagnitude > 0f)
            rb.velocity = moveInput.normalized * moveSpeed;
        else
            rb.velocity = Vector2.zero;
    }

    private void ReadMoveInput()
    {

        float x = 0f;
        float y = 0f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;
        if (Input.GetKey(KeyCode.W)) y += 1f;
        if (Input.GetKey(KeyCode.S)) y -= 1f;
        moveInput = new Vector2(x, y);

        if (x != 0f)
        {
            int newFacing;
            if (x > 0f) newFacing = 1;
            else newFacing = -1;

            if (newFacing != facing)
            {
                facing = newFacing;
                bodyTransform.localScale = new Vector3(facing, 1f, 1f) * 2f;
            }
        }

        bool hasHorizontal = x != 0f;
        bool hasVertical = y != 0f;
        if (moveAnim != null)
        {
            moveAnim.SetBool(IsMovingLR, hasHorizontal);
            moveAnim.SetBool(IsMovingU, !hasHorizontal && hasVertical && y > 0f);
            moveAnim.SetBool(IsMovingD, !hasHorizontal && hasVertical && y < 0f);
        }
    }

    private void HandleShootInput()
    {

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isFiring = false;
            return;
        }

        bool up = Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);

        float x = 0f;
        float y = 0f;
        if (right) x += 1f;
        if (left) x -= 1f;
        if (up) y += 1f;
        if (down) y -= 1f;
        Vector2 input = new Vector2(x, y);

        if (!isFiring)
        {
            bool pressed = Input.GetKeyDown(KeyCode.UpArrow) ||
                           Input.GetKeyDown(KeyCode.DownArrow) ||
                           Input.GetKeyDown(KeyCode.LeftArrow) ||
                           Input.GetKeyDown(KeyCode.RightArrow);
            if (pressed) isFiring = true;
            else return;
        }

        if (input != Vector2.zero)
        {
            if (eightWay) fireDirection = input.normalized;
            else fireDirection = Snap4(input);
        }
    }

    private static Vector2 Snap4(Vector2 v)
    {
        float absX = Mathf.Abs(v.x);
        float absY = Mathf.Abs(v.y);
        if (absX >= absY)
        {
            if (v.x != 0f) return new Vector2(Mathf.Sign(v.x), 0f);
            return new Vector2(0f, Mathf.Sign(v.y));
        }
        return new Vector2(0f, Mathf.Sign(v.y));
    }

    private void Fire()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.Fire(fireDirection);
    }

    private void UpdateShootAnim()
    {
        if (attackAnim == null) return;

        Vector2 dir = AnimDirection(fireDirection);
        attackAnim.SetBool("ShotUp", isFiring && dir == Vector2.up);
        attackAnim.SetBool("ShotDown", isFiring && dir == Vector2.down);
        attackAnim.SetBool("ShotLeft", isFiring && dir == Vector2.left);
        attackAnim.SetBool("ShotRight", isFiring && dir == Vector2.right);
    }

    private static Vector2 AnimDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return Vector2.zero;

        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);
        if (absX >= absY)
        {
            if (dir.x >= 0f) return Vector2.right;
            return Vector2.left;
        }
        if (dir.y >= 0f) return Vector2.up;
        return Vector2.down;
    }

    public void TakeDamage(int amount)
    {
        if (isDead || invulnTimer > 0f) return;

        hp -= amount;
        if (hp > 0)
        {
            if (hurtAudio != null) hurtAudio.Play();
            invulnTimer = invulnTime;
            StartHurt();
        }
        else
        {
            hp = 0;
            Die();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;
        if (isDead || invulnTimer > 0f) return;

        TakeDamage(enemy.Damage);

        Vector2 dir = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;
        Knockback playerKb = GetComponent<Knockback>();
        Knockback enemyKb = other.GetComponentInParent<Knockback>();
        if (playerKb != null) playerKb.Apply(dir);
        if (enemyKb != null) enemyKb.Apply(-dir);
    }

    private void StartHurt()
    {
        if (hurtAnim == null && action != null) hurtAnim = action.GetComponent<Animator>();
        if (hurtAnim != null) hurtAnim.SetBool(IsHurt, true);
        if (head != null) head.gameObject.SetActive(false);
        if (body != null) body.gameObject.SetActive(false);
        if (action != null) action.gameObject.SetActive(true);
        hurtTimer = hurtAnimLength;
    }

    private void EndHurt()
    {
        if (hurtAnim != null) hurtAnim.SetBool(IsHurt, false);
        if (head != null) head.gameObject.SetActive(true);
        if (body != null) body.gameObject.SetActive(true);
        if (action != null) action.gameObject.SetActive(false);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        hurtTimer = 0f;

        if (rb != null) rb.velocity = Vector2.zero;
        Knockback kb = GetComponent<Knockback>();
        if (kb != null) kb.enabled = false;

        if (head != null) head.gameObject.SetActive(false);
        if (body != null) body.gameObject.SetActive(false);
        if (action != null) action.gameObject.SetActive(true);

        if (deadAnim == null && action != null) deadAnim = action.GetComponent<Animator>();
        if (deadAnim != null)
        {
            deadAnim.SetBool(IsHurt, false);
            deadAnim.SetBool(IsDeadHash, true);
        }
        else
        {
            Debug.LogError("[Player.Die] 死亡动画 Animator 未赋值，请把 action 上的 Animator 拖给 deadAnim");
        }

        if (deathAudio != null) deathAudio.Play();
    }
}
