using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasicParamHandler:MonoBehaviour
{
    Type typeOfThisHandler = null;
    public UnityEvent onParamChanged = new();
    public int index = -1;
    public Type type
    {
        get
        {
            return typeOfThisHandler;
        }
        set
        {
            typeOfThisHandler = value;
        }
    }
}


