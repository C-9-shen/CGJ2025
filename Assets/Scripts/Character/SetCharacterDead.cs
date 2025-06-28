using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetCharacterDead : MonoBehaviour
{
    public List<DeadEvent> DeadEvents = new List<DeadEvent>();

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
                Debug.Log("Player entered death trigger!"); // 调试信息

                mainChar.Dead();

                foreach (var deadEvent in DeadEvents)
                {
                    if (deadEvent.EventIndex != -1)
                    {
                        if (deadEvent.TriggerOnce == false || deadEvent.Triggered == false)
                        {
                            deadEvent.Triggered = true;
                            deadEvent.TriggerEvent.Invoke();
                        }
                    }
                }
            }
        }
    }
}

[Serializable]
public class DeadEvent
{
    public int EventIndex = -1;
    public bool TriggerOnce = false;
    public bool Triggered = false;
    public UnityEvent TriggerEvent = new UnityEvent();
}