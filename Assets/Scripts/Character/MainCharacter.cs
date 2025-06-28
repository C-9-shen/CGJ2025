using UnityEngine;
using System.Collections;
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
    [Header("�������")]
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] protected LayerMask groundLayer;

    [Header("��ɫ����")]
    [SerializeField] protected CharacterStats stats;

    protected bool isFacingRight = true;
    public bool isGrounded;
    protected int jumpCount;
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;

    protected bool isDead = false;
    protected Transform RespawnPos;

    public CharacterStats Stats
    {
        get => stats;
        set
        {
            stats = value;
            ApplyStats();
        }
    }

    protected void Awake()
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
        if(animator==null)animator = GetComponent<Animator>();
        animator.SetFloat("SpeedY",rb.velocity.y);
        animator.SetFloat("SpeedX", Mathf.Abs(rb.velocity.x));
        animator.SetBool("IsGround", isGrounded);
        ApplyStats();
    }

    void Start()
    {
        
    }
    protected void ApplyStats()
    {
        rb.gravityScale = stats.gravityScale;
    }
    public void Move()
    {
        // 如果角色死亡，不处理移动
        if (isDead) return;
        
        rb.velocity = new Vector2(horizontalInput * stats.moveSpeed, rb.velocity.y);

        // Զתɫ
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
        // 如果角色死亡，不处理跳跃
        if (isDead) return;
        
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
        animator.SetFloat("SpeedY", rb.velocity.y);
        animator.SetFloat("SpeedX", Mathf.Abs(rb.velocity.x));
        animator.SetBool("IsGround", isGrounded);
        Debug.Log(rb.velocity);
        // 只在角色未死亡时处理输入
        if (!isDead)
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

    //API for character dead
    public void Dead()
    {
        if (isDead) return;
        else
        {
            isDead = true;
            Debug.Log("Character died! Attempting respawn..."); // 调试信息
            StartCoroutine(RespawnCoroutine());
        }
    }

    private System.Collections.IEnumerator RespawnCoroutine()
    {
        if (RespawnPos == null) 
        {
            Debug.LogWarning("No respawn position set! Please add SetRespawnPos trigger at starting point!");
            isDead = false; // 重置死亡状态避免卡住
            yield break;
        }
        
        Debug.Log($"Respawning at position: {RespawnPos.position}");
        
        // 停止所有物理运动
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // 暂时禁用物理模拟
        rb.simulated = false;
        
        // 等待一帧确保物理系统停止
        yield return null;
        
        // 强制设置位置
        transform.position = RespawnPos.position;
        rb.position = RespawnPos.position;
        
        // 重新启用物理模拟
        rb.simulated = true;
        
        // 重置状态
        isDead = false;
        isGrounded = true;
        jumpCount = stats.maxJumps;
        
        // 设置调试标志
        justRespawned = true;
        respawnTime = Time.time;
        
        Debug.Log($"Respawn completed. Current position: {transform.position}");
    }

    protected void Respawn()
    {
        // 这个方法现在被RespawnCoroutine替代，保留作为备用
        StartCoroutine(RespawnCoroutine());
    }

    // API for respawn position setting
    public void SetRespawnPos(Transform pos)
    {
        RespawnPos = pos; // 直接赋值Transform引用，而不是尝试访问null的position
        Debug.Log($"Respawn position set to: {pos.position}"); // 调试信息
    }
    
    // 调试方法：追踪位置变化
    private Vector3 lastPosition;
    private bool justRespawned = false;
    private float respawnTime;
    
    private void LateUpdate()
    {
        // 追踪重生后短时间内的位置变化
        if (justRespawned && Time.time - respawnTime < 1f)
        {
            if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
            {
                Debug.LogWarning($"Position changed after respawn! From {lastPosition} to {transform.position}");
            }
        }
        
        // 如果是重生后的第一帧，不检查距离变化
        if (justRespawned && Time.time - respawnTime > 0.1f)
        {
            justRespawned = false;
        }
        
        lastPosition = transform.position;
    }
}