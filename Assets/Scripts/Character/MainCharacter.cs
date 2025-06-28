using UnityEngine;
[System.Serializable]
public struct CharacterStats
{
    public float moveSpeed;
    public float jumpForce;
    public int maxJumps;
    public float gravityScale;

    public static CharacterStats Default => new CharacterStats
    {
        moveSpeed = 8f,
        jumpForce = 12f,
        maxJumps = 1,
        gravityScale = 3f
    };
}
public class MainCharacter : MonoBehaviour
{
    private static MainCharacter ins;
    public static MainCharacter Ins { get { return ins; } }
    [Header("基本组件")]
    [SerializeField] public Rigidbody2D rb;
    //[SerializeField] private Animator animator;
    [SerializeField] protected LayerMask groundLayer;

    [Header("角色属性")]
    [SerializeField] protected CharacterStats stats;

    protected bool isFacingRight = true;
    public bool isGrounded;
    protected int jumpCount; 
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;
    public CharacterStats Stats
    {
        get => stats;
        set
        {
            stats = value;
            ApplyStats();
        }
    }

    protected  void Awake()
    {
        if (ins == null)
        {
            ins = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        isGrounded = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        //if(animator==null)animator = GetComponent<Animator>();
        ApplyStats();
    }

    protected  void ApplyStats()
    {
        rb.gravityScale = stats.gravityScale;
    }
    public void Move()
    {
        rb.velocity = new Vector2(horizontalInput * stats.moveSpeed, rb.velocity.y);

        // 自动翻转角色朝向
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            bool shouldFaceRight = horizontalInput > 0;
            if (shouldFaceRight != isFacingRight)
            {
                Flip(shouldFaceRight);
            }
        }
    }

    public void SetMoveSpeed(float speed)
    {
        stats.moveSpeed = Mathf.Max(0, speed);
    }
    public void Jump()
    {
        if (CanJump())
        {
            rb.velocity = new Vector2(rb.velocity.x, stats.jumpForce);
            jumpCount--;
        }
    }

    protected bool CanJump()
    {
        UpdateGroundCheck();
        return jumpCount > 0;
    }

    protected void UpdateGroundCheck()
    {
        if (isGrounded && rb.velocity.y <= 0)
        {
            jumpCount = stats.maxJumps;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    public void ResetJumpCount()
    {
        jumpCount = stats.maxJumps;
    }
    public void Flip(bool faceRight)
    {
        if (isFacingRight == faceRight) return;

        isFacingRight = faceRight;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceRight ? 1 : -1);
        transform.localScale = scale;
    }

    public void SetFacingDirection(bool faceRight)
    {
        Flip(faceRight);
    }
    protected void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
        Move();
        UpdateGroundCheck();
    }
}