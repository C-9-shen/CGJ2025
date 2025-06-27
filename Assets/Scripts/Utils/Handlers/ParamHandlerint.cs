using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParamHandlerint : BasicParamHandler
{
    public int param;
    [HideInInspector]
    public int _param
    {
        get { return param; }
        set { SetParam(value); }
    }
    
    public UnityEvent<int> onParamChangedWithValue = new();

    public ParamHandlerint()
    {
        type = typeof(int);
    }

    public void SetParam(int value, bool invokeEvents = true)
    {
        param = value;
        if (invokeEvents)
        {
            onParamChanged.Invoke();
            onParamChangedWithValue.Invoke(value);
        }
    }

    public int GetParam()
    {
        return param;
    }

    public void OnValidate()
    {
        onParamChanged.Invoke();
        onParamChangedWithValue.Invoke(param);
    }
}