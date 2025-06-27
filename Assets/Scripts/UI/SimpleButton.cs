using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class SimpleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent<float> TransistionEvent;
    public float OverTransistTarget = 1.0f;
    public float ActivatingTransistTarget = 0.8f;
    public float DownTransistTarget = 0.6f;
    public float IdleTransistTarget = 0.0f;
    public float TransistFactor = 0.2f;
    public float TransistPct = 0f;
    public bool isOver = false;
    public bool isDown = false;
    public bool isActivating 
    { 
        set{
            _isActivating = value;
        }
        get
        {
            return _isActivating;
        }
    }
    [SerializeField]
    private bool _isActivating = false;
    private bool __isActivating = false;

    public UnityEvent MouseClickEvent;
    public UnityEvent MouseDownEvent;
    public UnityEvent MouseUpEvent;
    public UnityEvent MouseEnterEvent;
    public UnityEvent MouseExitEvent;
    public UnityEvent MouseOverEvent;
    public UnityEvent MouseClickEventActivating;
    public UnityEvent MouseClickEventDeactivating;
    public UnityEvent OnActivateEvent;
    public UnityEvent OnDeactivateEvent;
    public UnityEvent ActivatingEvent;
    public UnityEvent DeactivatingEvent;

    void Update()
    {
        float TransistTarget;
        if (isDown) TransistTarget = DownTransistTarget;
        else if (isOver) TransistTarget = OverTransistTarget;
        else if (isActivating) TransistTarget = ActivatingTransistTarget;
        else TransistTarget = IdleTransistTarget;
        TransistPct = Mathf.Lerp(TransistPct, TransistTarget, TransistFactor);
        if(Mathf.Abs(TransistPct - TransistTarget) < 0.01f) TransistPct = TransistTarget;
        TransistionEvent?.Invoke(TransistPct);
        if (isOver) MouseOverEvent?.Invoke();
        if (isActivating != __isActivating)
        {
            __isActivating = isActivating;
            if (isActivating) OnActivateEvent?.Invoke();
            else OnDeactivateEvent?.Invoke();
        }
        if(isActivating) ActivatingEvent?.Invoke();
        else DeactivatingEvent?.Invoke();
        
    }

    public void SetActivating(bool activating)
    {
        if (isActivating == activating) return;
        isActivating = activating;
        if (isActivating) OnActivateEvent?.Invoke();
        else OnDeactivateEvent?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MouseClickEvent?.Invoke();
        if (isActivating)
        {
            MouseClickEventActivating?.Invoke();
        }
        else
        {
            MouseClickEventDeactivating?.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MouseDownEvent?.Invoke();
        isDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MouseUpEvent?.Invoke();
        isDown = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseEnterEvent?.Invoke();
        isOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseExitEvent?.Invoke();
        isOver = false;
    }

    public void resetTransistPct()
    {
        isOver = false;
        TransistPct = 0;
        TransistionEvent?.Invoke(TransistPct);
    }
}
