using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;

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

    public bool _isOpen= false;
    public bool isOpen
    {
        get { return _isOpen; }
        set
        {
            _isOpen = value;
            if (value)
            {
                ButtonOpen?.Invoke();
            }
            else
            {
                ButtonClose?.Invoke();
            }
        }
    }

    public bool CanBeTouch=false;

    public UnityEvent ButtonOpen;
    public UnityEvent ButtonClose;
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
        if (CanBeTouch) 
        {
            if (Input.GetKeyDown(KeyCode.E)) 
            {
                switch (currentState)
                {
                    case SwitchState.OpenOnContact:
                        isOpen = !isOpen;
                        break;
                    case SwitchState.OpenThenClose:
                        if (targetObject != null)targetObject.isOpen = true;
                        isOpen = true;
                        timer = 0f;
                        break;
                    case SwitchState.AutoOpen:
                        // No action needed for AutoOpen state
                        break;
                }
            }
        }
        if (targetObject != null && targetObject.currentState == DoorState.Idle)
        {
            switch (currentState)
            {
                case SwitchState.OpenOnContact:
                    break;
                case SwitchState.OpenThenClose:
                    if (targetObject.isOpen)
                    {
                        timer += Time.deltaTime;
                        if (timer >= autoCloseDelay)
                        {
                            isOpen = false;
                            targetObject.isOpen = false;
                            timer = 0f;
                        }
                    }
                    break;
                case SwitchState.AutoOpen:
                    if (!targetObject.isOpen)
                    {
                        targetObject.isOpen = true;
                    }
                    break;
            }
        }else{
            if(currentState == SwitchState.OpenThenClose){
                if(isOpen){
                    timer += Time.deltaTime;
                    if (timer >= autoCloseDelay)
                    {
                        isOpen = false;
                        timer = 0f;
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CanBeTouch = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CanBeTouch = false;
        }
    }
}