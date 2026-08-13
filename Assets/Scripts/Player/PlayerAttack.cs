using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("射击参数")]
    [SerializeField, Tooltip("子弹预制体")] private GameObject bulletPrefab;
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField, Tooltip("勾选=八向（含对角），不勾=四向")] private bool eightWay = false;

    [Header("动画")]
    [SerializeField, Tooltip("玩家身上的 Animator（留空自动在子物体里找）")] private Animator anim;

    private bool isFiring;
    private Vector2 fireDirection = Vector2.up;
    private float fireTimer;

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        HandleInput();
        UpdateAnimator();

        if (isFiring)
        {
            fireTimer += Time.deltaTime;
            if (fireTimer >= fireInterval) { fireTimer = 0f; Fire(); }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        { isFiring = false; return; }

        bool up = Input.GetKey(KeyCode.UpArrow), down = Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow), right = Input.GetKey(KeyCode.RightArrow);
        Vector2 input = new Vector2((right?1:0)-(left?1:0), (up?1:0)-(down?1:0));

        if (!isFiring)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                isFiring = true;
            else return;
        }
        if (input != Vector2.zero)
            fireDirection = eightWay ? input.normalized : Snap4(input);
    }

    static Vector2 Snap4(Vector2 v)
    {
        if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
            return v.x != 0 ? new Vector2(Mathf.Sign(v.x), 0f) : new Vector2(0f, Mathf.Sign(v.y));
        return new Vector2(0f, Mathf.Sign(v.y));
    }

    void Fire()
    {
        if (bulletPrefab == null) return;
        GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        b.GetComponent<Bullet>()?.Fire(fireDirection);
    }

    // ========== 动画驱动 ==========
    void UpdateAnimator()
    {
        if (anim == null) return;

        Vector2 animDir = AnimDirection(fireDirection);

        anim.SetBool("ShotUp",    isFiring && animDir == Vector2.up);
        anim.SetBool("ShotDown",  isFiring && animDir == Vector2.down);
        anim.SetBool("ShotLeft",  isFiring && animDir == Vector2.left);
        anim.SetBool("ShotRight", isFiring && animDir == Vector2.right);
    }

    static Vector2 AnimDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return Vector2.zero;
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y)) return dir.x >= 0 ? Vector2.right : Vector2.left;
        return dir.y >= 0 ? Vector2.up : Vector2.down;
    }
}
