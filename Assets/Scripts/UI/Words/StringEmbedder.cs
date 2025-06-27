using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class StringEmbedder : MonoBehaviour
{

    public bool ApplyToTMP = false;
    public int NumOfVars = 0;
    public List<BasicParamHandler> Handlers = new();
    public HandlerType TypeOfHandler = HandlerType.String;
    public enum HandlerType
    {
        Int,
        Float,
        String,
        Bool,
        Vector2,
        Vector3,
    }
    public UnityEvent OnDataChanged = new UnityEvent();
    [TextArea(10, 10)]
    public string TextToEmbed;

    void Start()
    {
        foreach (var handler in Handlers)
        {
            if (handler != null)
            {
                handler.onParamChanged.AddListener(() => OnDataChanged.Invoke());
            }
        }
    }

    void Update()
    {
        if(ApplyToTMP)
        {
            TMP_Text tmp = null;
            TryGetComponent<TMP_Text>(out tmp);
            if (tmp != null) tmp.text = ProcessText(TextToEmbed);
            TMP_InputField inputField = null;
            TryGetComponent<TMP_InputField>(out inputField);
            if (inputField != null)
            {
                inputField.text = ProcessText(TextToEmbed);
                inputField.onValueChanged.Invoke(inputField.text);
            }
        }
    }

    public void EditorUpdate()
    {
        for(int i = Handlers.Count-1; i >= 0; i--)
        {
            if (Handlers[i] == null)
            {
                Handlers.RemoveAt(i);
                GameObject newHandler = new GameObject("ParamHandler_" + i);
                newHandler.transform.SetParent(transform);
                BasicParamHandler handler = (BasicParamHandler)newHandler.AddComponent(GetHandlerFromHandlerType(TypeOfHandler));
                handler.index = i;
                Handlers.Add(handler);
            }
        }
        while (Handlers.Count < NumOfVars)
        {
            GameObject newHandler = new GameObject("ParamHandler_" + Handlers.Count);
            newHandler.transform.SetParent(transform);
            BasicParamHandler handler = (BasicParamHandler)newHandler.AddComponent(GetHandlerFromHandlerType(TypeOfHandler));
            handler.index = Handlers.Count;
            Handlers.Add(handler);
        }
        while (Handlers.Count > NumOfVars)
        {
            DestroyImmediate(Handlers[Handlers.Count - 1].gameObject);
            Handlers.RemoveAt(Handlers.Count - 1);
        }
    }

    Type GetHandlerFromHandlerType(HandlerType type)
    {
        return type switch
        {
            HandlerType.Int => typeof(ParamHandlerint),
            HandlerType.Float => typeof(ParamHandlerfloat),
            HandlerType.String => typeof(ParamHandlerstring),
            HandlerType.Bool => typeof(ParamHandlerbool),
            HandlerType.Vector2 => typeof(ParamHandlerVector2),
            HandlerType.Vector3 => typeof(ParamHandlerVector3),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public string ProcessText(string inputText)
    {
        string result = inputText;
        
        // 查找并替换所有 <vars=index> 标签
        for (int i = 0; i < Handlers.Count; i++)
        {
            string tagToReplace = $"<vars={i}>";
            if (result.Contains(tagToReplace) && Handlers[i] != null)
            {
                string replacementValue = GetValueFromHandler(Handlers[i]);
                result = result.Replace(tagToReplace, replacementValue);
            }
        }
        
        return result;
    }

    private string GetValueFromHandler(BasicParamHandler handler)
    {
        return handler switch
        {
            ParamHandlerint intHandler => intHandler.param.ToString(),
            ParamHandlerfloat floatHandler => floatHandler.param.ToString(),
            ParamHandlerstring stringHandler => stringHandler.param,
            ParamHandlerbool boolHandler => boolHandler.param.ToString(),
            ParamHandlerVector2 vector2Handler => vector2Handler.param.ToString(),
            ParamHandlerVector3 vector3Handler => vector3Handler.param.ToString(),
            _ => ""
        };
    }
}
