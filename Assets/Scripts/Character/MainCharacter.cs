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

    [Header("地面检测")]
    [SerializeField] protected float groundCheckDistance = 0.2f; // 射线检测距离
    [SerializeField] protected Vector2 groundCheckOffset = Vector2.zero; // 检测点偏移
    [SerializeField] protected float groundCheckWidth = 0.8f; // 检测宽度
    [SerializeField] protected int raycastCount = 3; // 射线数量

    [Header("侧面检测")]
    [SerializeField] protected float sideCheckDistance = 0.1f; // 侧面检测距离
    [SerializeField] protected Vector2 sideCheckOffset = Vector2.zero; // 侧面检测偏移
    [SerializeField] protected float sideCheckHeight = 0.8f; // 侧面检测高度
    [SerializeField] protected int sideRaycastCount = 3; // 侧面射线数量

    protected bool isFacingRight = true;
    public bool isGrounded;
    protected bool isBlockedLeft = false; // 左侧是否被阻挡
    protected bool isBlockedRight = false; // 右侧是否被阻挡
    protected int jumpCount;
    private float horizontalInput;
    private bool jumpPressed;
    private bool jumpHeld;

    protected bool isDead = false;
    protected Transform RespawnPos;

    public GameObject GameManagertemp;

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
        GameManagertemp = GameObject.FindGameObjectWithTag("GM");
    }
    protected void ApplyStats()
    {
        rb.gravityScale = stats.gravityScale;
    }
    public void Move()
    {
        // 如果角色死亡，不处理移动
        if (isDead) return;
        
        // 检测侧面阻挡
        UpdateSideCheck();
        
        // 根据侧面检测结果限制输入
        float processedInput = horizontalInput;
        
        // 如果向左移动但左侧被阻挡，禁用左方向输入
        if (processedInput < 0 && isBlockedLeft)
        {
            processedInput = 0;
        }
        // 如果向右移动但右侧被阻挡，禁用右方向输入
        else if (processedInput > 0 && isBlockedRight)
        {
            processedInput = 0;
        }
        
        rb.velocity = new Vector2(Mathf.Pow(processedInput, 3) * stats.moveSpeed, rb.velocity.y);

        // 转向角色
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
        // 使用射线检测地面
        isGrounded = CheckSideWithRaycast(Vector2.down,groundCheckOffset);
        
        if (isGrounded && rb.velocity.y <= 0)
        {
            jumpCount = stats.maxJumps;
        }
    }
    
    protected void UpdateSideCheck()
    {
        // 检测左侧
        isBlockedLeft = CheckSideWithRaycast(Vector2.left,sideCheckOffset);
        
        // 检测右侧
        isBlockedRight = CheckSideWithRaycast(Vector2.right,sideCheckOffset);
    }
    
    protected bool CheckSideWithRaycast(Vector2 direction,Vector2 Offset)
    {
        Vector2 startPos = (Vector2)transform.position + Offset;
        
        // 多条射线检测，覆盖角色高度
        for (int i = 0; i < sideRaycastCount; i++)
        {
            float t = sideRaycastCount > 1 ? (float)i / (sideRaycastCount - 1) : 0.5f;
            Vector2 raycastPos = startPos + Vector2.down * sideCheckHeight * 0.5f + Vector2.up * sideCheckHeight * t;
            
            RaycastHit2D hit = Physics2D.Raycast(raycastPos, direction, sideCheckDistance, groundLayer);
            
            if (hit.collider != null)
            {
                return true;
            }
        }
        
        return false;
    }
    
    // 移除碰撞检测方法
    /*
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
    */
    
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

        // 更新检测
        UpdateGroundCheck();
        UpdateSideCheck();

        // 只在角色未死亡且输入启用时处理输入
        if (!isDead && GameManager.InputEnabled)
        {
            horizontalInput = Input.GetAxis("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
            Move();
        }
        else if (!GameManager.InputEnabled)
        {
            // 输入被禁用时，停止移动
            horizontalInput = 0;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            GameManagertemp.GetComponent<GameManager>().SwitchScene("Tutorial");
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            GameManagertemp.GetComponent<GameManager>().SwitchScene("Interior");
        }
        if (Input.GetKeyDown(KeyCode.O))
        { 
            GameManagertemp.GetComponent<GameManager>().SwitchScene("Chase");
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
    
    // 可视化调试射线
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Vector2 startPos = (Vector2)transform.position + groundCheckOffset;
            
            // 绘制地面检测射线
            Gizmos.color = isGrounded ? Color.green : Color.red;
            for (int i = 0; i < raycastCount; i++)
            {
                float t = raycastCount > 1 ? (float)i / (raycastCount - 1) : 0.5f;
                Vector2 raycastPos = startPos + Vector2.left * groundCheckWidth * 0.5f + Vector2.right * groundCheckWidth * t;
                
                Gizmos.DrawRay(raycastPos, Vector2.down * groundCheckDistance);
            }
            
            // 绘制地面检测范围
            Gizmos.color = Color.yellow;
            Vector2 leftPoint = startPos + Vector2.left * groundCheckWidth * 0.5f;
            Vector2 rightPoint = startPos + Vector2.right * groundCheckWidth * 0.5f;
            Gizmos.DrawLine(leftPoint, rightPoint);
            
            // 绘制侧面检测射线
            Vector2 sideStartPos = (Vector2)transform.position + sideCheckOffset;
            
            // 左侧检测
            Gizmos.color = isBlockedLeft ? Color.red : Color.blue;
            for (int i = 0; i < sideRaycastCount; i++)
            {
                float t = sideRaycastCount > 1 ? (float)i / (sideRaycastCount - 1) : 0.5f;
                Vector2 raycastPos = sideStartPos + Vector2.down * sideCheckHeight * 0.5f + Vector2.up * sideCheckHeight * t;
                Gizmos.DrawRay(raycastPos, Vector2.left * sideCheckDistance);
            }
            
            // 右侧检测
            Gizmos.color = isBlockedRight ? Color.red : Color.blue;
            for (int i = 0; i < sideRaycastCount; i++)
            {
                float t = sideRaycastCount > 1 ? (float)i / (sideRaycastCount - 1) : 0.5f;
                Vector2 raycastPos = sideStartPos + Vector2.down * sideCheckHeight * 0.5f + Vector2.up * sideCheckHeight * t;
                Gizmos.DrawRay(raycastPos, Vector2.right * sideCheckDistance);
            }
            
            // 绘制侧面检测范围
            Gizmos.color = Color.cyan;
            Vector2 topPoint = sideStartPos + Vector2.up * sideCheckHeight * 0.5f;
            Vector2 bottomPoint = sideStartPos + Vector2.down * sideCheckHeight * 0.5f;
            Gizmos.DrawLine(topPoint, bottomPoint);
        }
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