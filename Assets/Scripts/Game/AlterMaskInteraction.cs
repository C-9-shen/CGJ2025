using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlterMaskInteraction : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// 设置SpriteRenderer的mask interaction模式为None
    /// </summary>
    public void SetMaskInteractionToNone()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        }
        else
        {
            Debug.LogError("SpriteRenderer is null. Cannot set mask interaction.");
        }
    }
}
