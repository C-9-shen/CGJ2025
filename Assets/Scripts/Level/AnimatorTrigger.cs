using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTrigger : MonoBehaviour
{
    public Animator Animator;
    public List<AnimatorTriggerEvent> TriggerEvents = new List<AnimatorTriggerEvent>();

    public void TriggerEvent()
    {
        foreach (var triggerEvent in TriggerEvents)
        {
            if (triggerEvent.Type == AnimatorTriggerEvent.TriggerType.Trigger)
            {
                if (triggerEvent.TriggerOnce == false || triggerEvent.Triggered == false)
                {
                    Animator.SetTrigger(triggerEvent.TriggerName);
                    triggerEvent.Triggered = true;
                }
            }
            else if (triggerEvent.Type == AnimatorTriggerEvent.TriggerType.SetBool)
            {
                Animator.SetBool(triggerEvent.TriggerName, triggerEvent.BoolValue);
            }
            else if (triggerEvent.Type == AnimatorTriggerEvent.TriggerType.SetInt)
            {
                Animator.SetInteger(triggerEvent.TriggerName, triggerEvent.IntValue);
            }
            else if (triggerEvent.Type == AnimatorTriggerEvent.TriggerType.SetFloat)
            {
                Animator.SetFloat(triggerEvent.TriggerName, triggerEvent.FloatValue);
            }
        }
    }

    [Serializable]
    public class AnimatorTriggerEvent
    {
        public string TriggerName;
        public bool TriggerOnce = false;
        public TriggerType Type = TriggerType.None;
        public int IntValue = 0;
        public float FloatValue = 0f;
        public bool BoolValue = false;
        [HideInInspector] public bool Triggered = false;

        public enum TriggerType
        {
            None,
            Trigger,
            SetBool,
            SetInt,
            SetFloat
        }
    }
}
