using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParamHandlerfloat : BasicParamHandler
{
    public float param;
    [HideInInspector]
    public float _param
    {
        get { return param; }
        set { SetParam(value); }
    }
    public UnityEvent<float> onParamChangedWithValue = new();

    public ParamHandlerfloat()
    {
        type = typeof(float);
    }

    public void SetParam(float value, bool invokeEvents = true)
    {
        param = value;
        if (invokeEvents)
        {
            onParamChanged.Invoke();
            onParamChangedWithValue.Invoke(value);
        }
    }

    public float GetParam()
    {
        return param;
    }

    public void OnValidate()
    {
        onParamChanged.Invoke();
        onParamChangedWithValue.Invoke(param);
    }
}