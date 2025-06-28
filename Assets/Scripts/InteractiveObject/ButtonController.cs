using UnityEngine;

public enum SwitchState
{
    OpenOnContact,
    OpenThenClose,
    AutoOpen 
}

public class ButtonController : MonoBehaviour
{
    public DoorController targetObject;
    public SwitchState currentState = SwitchState.OpenOnContact; 
    public float autoCloseDelay = 5f; 

    private bool isOpen = false; 
    private float timer = 0f;

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is not assigned.");
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case SwitchState.OpenOnContact:
                if (isOpen)
                {
                    targetObject.isOpen = true;
                }
                else { targetObject.isOpen = false; }
                break;
            case SwitchState.OpenThenClose:
                if (isOpen)
                {
                    timer += Time.deltaTime;
                    if (timer >= autoCloseDelay)
                    {
                        targetObject.isOpen = false;
                        isOpen = false;
                        timer = 0f;
                    }
                }
                break;
            case SwitchState.AutoOpen:
                if (!isOpen)
                {
                    targetObject.isOpen = true;
                    isOpen = true;
                }
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            switch (currentState)
            {
                case SwitchState.OpenOnContact:
                    if (isOpen == false)
                    {
                        targetObject.isOpen = true;
                        isOpen = true;
                    }
                    else 
                    {
                        targetObject.isOpen = false;
                        isOpen = false;
                    }
                        break;
                case SwitchState.OpenThenClose:
                    targetObject.isOpen = true;
                    isOpen = true;
                    timer = 0f;
                    break;
                case SwitchState.AutoOpen:
                    // No action needed for AutoOpen state
                    break;
            }
        }
    }
}