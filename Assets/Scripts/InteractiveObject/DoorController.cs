using UnityEngine;
using UnityEngine.Events;

public enum DoorState
{
    Idle,
    FollowPlayerY,
    JumpToAvoidPlayer
}

public class DoorController : MonoBehaviour
{
    public GameObject player;
    public float activeRange = 5f;
    public DoorState currentState = DoorState.Idle;
    public bool isOpen = false;
    public float jumpHeight = 3f;
    public float jumpDuration = 0.3f;
    public float followSpeed = 2f;

    private bool isPlayerInRange = false;
    private Vector3 initialPosition;
    private float jumpSpeed;
    private float returnSpeed;
    private bool isJumping = false;
    private bool isReturning = false;
    private float jumpStartTime;
    private float returnStartTime;
    private float tolerance = 0.05f;

    public UnityEvent OnDoorOpen;
    public UnityEvent OnDoorClose;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        initialPosition = transform.position;
        jumpSpeed = jumpHeight / jumpDuration;
        returnSpeed = jumpHeight / jumpDuration;
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            isPlayerInRange = distance <= activeRange;
            if (isOpen && currentState == DoorState.Idle)
            {
                GetComponent<Collider2D>().enabled = false;
            }
            else if (isOpen && currentState != DoorState.Idle)
            {
                GetComponent<Collider2D>().enabled = true;
            }
            else if (!isOpen) 
            {
                GetComponent<Collider2D>().enabled = true;
            }

                switch (currentState)
                {
                    case DoorState.FollowPlayerY:
                        if (isPlayerInRange)
                        {
                            FollowPlayerY();
                        }
                        else if (!isPlayerInRange)
                        {
                        transform.position = initialPosition;
                    }
                        break;
                    case DoorState.JumpToAvoidPlayer:
                        if (isPlayerInRange && !isJumping && !isReturning)
                        {
                            StartJump();
                        }
                        else if (!isPlayerInRange && !isReturning)
                        {
                            StartReturn();
                        }
                        if (isJumping)
                        {
                            PerformJump();
                        }
                        if (isReturning)
                        {
                            PerformReturn();
                        }
                        break;
                    case DoorState.Idle:
                    default:
                        break;
                }
        }
    }

    private void FollowPlayerY()
    {
        transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
    }

    private void StartJump()
    {
        if (this.transform.position.y == initialPosition.y)
        {
            jumpStartTime = Time.time;
            isJumping = true;
        }
    }

    private void PerformJump()
    {
        float elapsedTime = Time.time - jumpStartTime;
        if (elapsedTime < jumpDuration)
        {
            float deltaY = jumpSpeed * Time.deltaTime;
            transform.position += new Vector3(0, deltaY, 0);
        }
        else
        {
            // ȷ���ŵ���Ŀ��߶�
            transform.position = new Vector3(transform.position.x, initialPosition.y + jumpHeight, transform.position.z);
            isJumping = false;
        }
    }

    private void StartReturn()
    {
        returnStartTime = Time.time;
        isReturning = true;
    }

    private void PerformReturn()
    {
        float elapsedTime = Time.time - returnStartTime;
        if (elapsedTime < jumpDuration)
        {
            float deltaY = -returnSpeed * Time.deltaTime;
            transform.position += new Vector3(0, deltaY, 0);

            // ����Ƿ�ӽ���ʼλ��
            if (Mathf.Abs(transform.position.y - initialPosition.y) < tolerance)
            {
                transform.position = initialPosition; // ȷ���Żص���ʼλ��
                isReturning = false;
            }
        }
        else
        {
            // ȷ���Żص���ʼλ��
            transform.position = initialPosition;
            isReturning = false;
        }
    }

    public void SetState(DoorState state)
    {
        currentState = state;
    }
    
    public void ToggleDoor()
    {

        isOpen = !isOpen;
        
        // Invoke the appropriate event based on door state
        if (isOpen)
        {
            if (OnDoorOpen != null)
                OnDoorOpen?.Invoke();
        }
        else
        {
            if (OnDoorClose != null)
                OnDoorClose?.Invoke();
        }
        
    }

    public void SetDoor(bool open){
        isOpen = open;
        if (isOpen)
        {
            if (OnDoorOpen != null)
                OnDoorOpen?.Invoke();
        }
        else
        {
            if (OnDoorClose != null)
                OnDoorClose?.Invoke();
        }
    }
}