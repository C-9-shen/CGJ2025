using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GeneralTrigger : MonoBehaviour
{
    protected new Collider2D collider;

    public List<TriggerEvent> TriggerEvents = new List<TriggerEvent>();

    protected void Awake()
    {
        collider = GetComponent<Collider2D>();
        if (collider.isTrigger == false)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测进入触发器的对象是否是Player
        if (other.gameObject.CompareTag("Player"))
        {
            MainCharacter mainChar = other.GetComponent<MainCharacter>();
            if (mainChar != null)
            {
                Debug.Log("Player entered general trigger!"); // 调试信息
                foreach (TriggerEvent triggerEvent in TriggerEvents)
                {
                    if (triggerEvent.EventIndex != -1)
                    {
                        if (triggerEvent.TriggerOnce == false || triggerEvent.Triggered == false)
                        {
                            triggerEvent.Triggered = true;
                            triggerEvent.triggerEvent.Invoke();
                        }
                    }
                }
            }
        }
    }
}

[Serializable]
public class TriggerEvent
{
    public int EventIndex = -1;
    public bool TriggerOnce = false;
    public bool Triggered = false;
    public UnityEvent triggerEvent = new UnityEvent();
};