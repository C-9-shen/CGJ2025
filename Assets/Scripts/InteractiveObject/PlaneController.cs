using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public enum PlatformState
{
    Normal,
    Fall,
    Lift
}

public class PlatformController : MonoBehaviour
{
    public PlatformState currentState = PlatformState.Normal;
    public Vector3[] waypoints;
    public float moveSpeed = 2f;
    public float fallSpeed = 5f;
    public float liftHeight = 3f;
    public float liftSpeed = 2f;
    public float delayBeforeFall = 2f;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    public bool isFalling = false;
    public float topThreshold = 0.7f;
    private bool isLifting = false; 
    private Vector3 originalPosition; 
    private Rigidbody2D rb; 

    public UnityEvent StepOnPlatform;
    public UnityEvent LeavePlatform;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position; 
        if (currentState == PlatformState.Normal)
        {
            StartCoroutine(MovePlatform());
        }
    }

    void Update()
    {
        if (currentState == PlatformState.Normal && isMoving)
        {
            MovePlatform();
        }
        else if (currentState == PlatformState.Fall && isFalling)
        {
            FallPlatform();
        }
        else if (currentState == PlatformState.Lift && isLifting)
        {
            LiftPlatform();
        }
    }

    public void ResetPos()
    {
        transform.position = originalPosition;
        currentWaypointIndex = 0;
        isFalling = false;
        isLifting = false;
        isMoving = false;
        rb.velocity = Vector2.zero;
    }

    private IEnumerator MovePlatform()
    {
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex], moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex]) < 0.1f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            yield return null;
        }
    }

    private void FallPlatform()
    {
        rb.velocity = new Vector2(rb.velocity.x, -fallSpeed);
    }

    private void LiftPlatform()
    {
        if (transform.position.y < originalPosition.y + liftHeight)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + liftSpeed * Time.deltaTime, transform.position.z);
        }
        else
        {
            isLifting = false;
            transform.position = new Vector3(transform.position.x, originalPosition.y + liftHeight, transform.position.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector3 normal = contact.normal;
                if (normal.y < -topThreshold)
                {
                    StepOnPlatform?.Invoke();
                    
                    // For both Normal and Lift states, parent the player to the platform
                    if (currentState == PlatformState.Normal || currentState == PlatformState.Lift)
                    {
                        collision.transform.SetParent(transform);
                    }
                    
                    if (currentState == PlatformState.Fall)
                    {
                        isFalling = true;
                        Invoke("StopFalling", delayBeforeFall);
                    }
                    else if (currentState == PlatformState.Lift)
                    {
                        isLifting = true;
                    }
                }
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            LeavePlatform?.Invoke();
            
            // For both Normal and Lift states, unparent the player when they leave
            if (currentState == PlatformState.Normal || currentState == PlatformState.Lift)
            {
                collision.transform.SetParent(null);
            }
        }
    }
    private void StopFalling()
    {
        isFalling = false;
        rb.velocity = Vector2.zero;
    }

    public void ChangeState(PlatformState newState)
    {
        if (currentState == PlatformState.Normal) 
        {
            isMoving = false;
            StopCoroutine(MovePlatform());
        }
        currentState = newState;
        if (currentState == PlatformState.Normal)
        {
            StartCoroutine(MovePlatform());
        }
        else if (currentState == PlatformState.Fall)
        {
            isFalling = false;
        }
        else if (currentState == PlatformState.Lift)
        {
            isLifting = false;
        }
    }
}