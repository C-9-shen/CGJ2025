using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRespawnPos : MonoBehaviour
{
    protected new Collider2D collider;

    protected void Awake()
    {
        collider = GetComponent<Collider2D>();
        if (collider.isTrigger == false)
        { 
            collider.isTrigger = true;
        }
    }

    void Start()
    {
        
    }

    // 使用正确的触发器事件
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测进入触发器的对象是否是Player
        if (other.gameObject.CompareTag("Player"))
        {
            MainCharacter mainChar = other.GetComponent<MainCharacter>();
            if (mainChar != null)
            {
                Debug.Log("Player entered respawn trigger!"); // 调试信息
                mainChar.SetRespawnPos(transform);
            }
        }
    }
}
