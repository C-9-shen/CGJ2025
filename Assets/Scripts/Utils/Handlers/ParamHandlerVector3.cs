using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParamHandlerVector3 : BasicParamHandler
{
    public Vector3 param;
    [HideInInspector]
    public Vector3 _param
    {
        get { return param; }
        set { SetParam(value); }
    }
    public UnityEvent<Vector3> onParamChangedWithValue = new();

    public ParamHandlerVector3()
    {
        type = typeof(Vector3);
    }

    public void SetParam(Vector3 value, bool invokeEvents = true)
    {
        param = value;
        if (invokeEvents)
        {
            onParamChanged.Invoke();
            onParamChangedWithValue.Invoke(value);
        }
    }

    public Vector3 GetParam()
    {
        return param;
    }

    public void OnValidate()
    {
        onParamChanged.Invoke();
        onParamChangedWithValue.Invoke(param);
    }
}