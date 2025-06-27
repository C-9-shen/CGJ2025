using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParamHandlerstring : BasicParamHandler
{
    public string param;
    [HideInInspector]
    public string _param
    {
        get { return param; }
        set { SetParam(value); }
    }
    public UnityEvent<string> onParamChangedWithValue = new();

    public ParamHandlerstring()
    {
        type = typeof(string);
    }

    public void SetParam(string value, bool invokeEvents = true)
    {
        param = value;
        if (invokeEvents)
        {
            onParamChanged.Invoke();
            onParamChangedWithValue.Invoke(value);
        }
    }

    public string GetParam()
    {
        return param;
    }

    public void OnValidate()
    {
        onParamChanged.Invoke();
        onParamChangedWithValue.Invoke(param);
    }
}