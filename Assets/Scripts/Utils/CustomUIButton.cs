using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CustomUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent<float> TransistionEvent;
    public float TransistFactor = 0.2f;
    public float TransistPct = 0f;
    public bool isOver = false;

    public UnityEvent uiClickEventInt;
    public UnityEvent uiEnterEventInt;
    public UnityEvent uiExitEventInt;
    
    

    private Button button;

    void Update()
    {
        if(isOver) TransistPct = TransistPct*(1-TransistFactor) + TransistFactor;
        else TransistPct = TransistPct*(1-TransistFactor);
        if (TransistPct < 0.001f) TransistPct = 0;
        if (TransistPct > 0.999f) TransistPct = 1;
        TransistionEvent?.Invoke(TransistPct);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => uiClickEventInt?.Invoke());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        uiEnterEventInt?.Invoke();
        isOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiExitEventInt?.Invoke();
        isOver = false;
    }

    public void resetTransistPct()
    {
        isOver = false;
        TransistPct = 0;
        TransistionEvent?.Invoke(TransistPct);
    }
}
