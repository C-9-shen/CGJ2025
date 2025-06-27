using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScrollTextContent : MonoBehaviour
{
    public RectTransform targetContent;
    public bool autoUpdate = true;
    public float padding = 10f;
    public bool updateWidth = true;
    public bool updateHeight = true;
    
    public TMP_Text tmpText;
    private string lastText = "";
    private Vector2 lastPreferredSize;

    public void Update()
    {
        if (autoUpdate && tmpText != null)
        {
            bool textChanged = tmpText.text != lastText;
            Vector2 currentPreferredSize = tmpText.GetPreferredValues();
            bool sizeChanged = currentPreferredSize != lastPreferredSize;
            
            if (textChanged || sizeChanged)
            {
                UpdateContentSize();
                lastText = tmpText.text;
                lastPreferredSize = currentPreferredSize;
            }
        }
    }

    [ContextMenu("Update Content Size")]
    public void UpdateContentSize()
    {
        if (tmpText == null || targetContent == null) return;

        // 强制刷新文本布局
        tmpText.ForceMeshUpdate();
        
        // 获取文本的首选尺寸
        Vector2 preferredSize = tmpText.GetPreferredValues();
        
        // 获取当前content的尺寸
        Vector2 currentSize = targetContent.sizeDelta;
        
        // 计算新的尺寸
        Vector2 newSize = currentSize;
        
        if (updateWidth)
        {
            newSize.x = preferredSize.x + padding * 2f;
        }
        
        if (updateHeight)
        {
            newSize.y = preferredSize.y + padding * 2f;
        }
        
        // 应用新尺寸
        targetContent.sizeDelta = newSize;
    }

    public void SetPadding(float newPadding)
    {
        padding = newPadding;
        UpdateContentSize();
    }

}
