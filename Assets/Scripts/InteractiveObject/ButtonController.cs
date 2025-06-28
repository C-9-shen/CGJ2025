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
    public bool CanBeTouch=false;

    public UnityEvent ButtonCut;
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
                        ButtonCut.Invoke();
                        break;
                    case SwitchState.OpenThenClose:
                        ButtonCut.Invoke();
                        timer = 0f;
                        break;
                    case SwitchState.AutoOpen:
                        break;
                }
            }
        }
        if (targetObject.currentState == DoorState.Idle)
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
                            ButtonCut.Invoke();
                            timer = 0f;
                        }
                    }
                    break;
                case SwitchState.AutoOpen:
                    if (!targetObject.isOpen)
                    {
                        ButtonCut.Invoke();
                    }
                    break;
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
}