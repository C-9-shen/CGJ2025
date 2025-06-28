using UnityEngine;

public enum FlagState
{
    Idle,
    MoveTowardsPlayer,
    MoveAwayFromPlayer
}

public class FlagController : MonoBehaviour
{
    [SerializeField] public float activeRange = 5f;
    [SerializeField] public float moveSpeed;
    [SerializeField] public GameObject player;
    [SerializeField] public FlagState currentState = FlagState.Idle;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public Collider2D flagCollider;
    private bool isPlayerInRange = false;
    private bool isTounched = false;

    void Start()
    {
        isTounched = false;
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");

        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>(); 
        }
    }

    void Update()
    {
        if (player != null&&!isTounched)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            isPlayerInRange = distance <= activeRange;

            if (isPlayerInRange)
            {
                switch (currentState)
                {
                    case FlagState.MoveTowardsPlayer:
                        MoveTowardsPlayer();
                        break;
                    case FlagState.MoveAwayFromPlayer:
                        MoveAwayFromPlayer();
                        break;
                    case FlagState.Idle:
                    default:
                        break;
                }
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player.transform.position.x > this.transform.position.x)
        {
            rb.velocity = new Vector2(Mathf.Abs(player.GetComponent<MainCharacter>().rb.velocity.x), rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(-Mathf.Abs(player.GetComponent<MainCharacter>().rb.velocity.x), rb.velocity.y);
        }    
    }

    private void MoveAwayFromPlayer()
    {
        if (player.transform.position.x < this.transform.position.x)
        {
            rb.velocity = new Vector2(Mathf.Abs(player.GetComponent<MainCharacter>().rb.velocity.x), rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(-Mathf.Abs(player.GetComponent<MainCharacter>().rb.velocity.x), rb.velocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTounched = true;
            DisableComponents();
            Debug.Log("Player has reached the flag. Game Over.");
            // 可以在这里调用游戏胜利的逻辑
        }
    }
    private void DisableComponents()
    {
        if (flagCollider != null)
        {
            flagCollider.enabled = false; 
        }
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
    }
    public void SetState(FlagState state)
    {
        currentState = state;
    }
}