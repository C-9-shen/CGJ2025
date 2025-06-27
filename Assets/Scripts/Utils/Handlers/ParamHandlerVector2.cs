using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParamHandlerVector2 : BasicParamHandler
{
    public Vector2 param;
    [HideInInspector]
    public Vector2 _param
    {
        get { return param; }
        set { SetParam(value); }
    }
    public UnityEvent<Vector2> onParamChangedWithValue = new();

    public ParamHandlerVector2()
    {
        type = typeof(Vector2);
    }

    public void SetParam(Vector2 value, bool invokeEvents = true)
    {
        param = value;
        if (invokeEvents)
        {
            onParamChanged.Invoke();
            onParamChangedWithValue.Invoke(value);
        }
    }

    public Vector2 GetParam()
    {
        return param;
    }

    public void OnValidate()
    {
        onParamChanged.Invoke();
        onParamChangedWithValue.Invoke(param);
    }
}