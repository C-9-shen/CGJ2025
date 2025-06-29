using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTrigger : MonoBehaviour
{
    public UnityEvent OnEnableEvent;
    public UnityEvent OnDisableEvent;
    public UnityEvent OnStartEvent;
    public UnityEvent OnUpdateEvnet;
    public UnityEvent OnEnterTriggerEvent;
    public UnityEvent OnExitTriggerEvent;

    private void OnEnable()
    {
        OnEnableEvent?.Invoke();
    }

    private void OnDisable()
    {
        OnDisableEvent?.Invoke();
    }

    private void Start()
    {
        OnStartEvent?.Invoke();
    }

    private void Update()
    {
        OnUpdateEvnet?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnEnterTriggerEvent?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        OnExitTriggerEvent?.Invoke();
    }


}
