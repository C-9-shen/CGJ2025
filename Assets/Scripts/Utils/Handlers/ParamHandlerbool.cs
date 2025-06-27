using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParamHandlerbool : BasicParamHandler
{
    public bool param;
    [HideInInspector]
    public bool _param
    {
        get { return param; }
        set { SetParam(value); }
    }
    public UnityEvent<bool> onParamChangedWithValue = new();

    public ParamHandlerbool()
    {
        type = typeof(bool);
    }

    public void SetParam(bool value, bool invokeEvents = true)
    {
        param = value;
        if (invokeEvents)
        {
            onParamChanged.Invoke();
            onParamChangedWithValue.Invoke(value);
        }
    }

    public bool GetParam()
    {
        return param;
    }

    public void OnValidate()
    {
        onParamChanged.Invoke();
        onParamChangedWithValue.Invoke(param);
    }
}