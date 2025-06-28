using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogBox : MonoBehaviour
{
    public DialogList DialogList;
    public List<DialogEvent> DialogEvents = new List<DialogEvent>();
    public TMPPlus TargetText;
    public Animator TransistionAnimator;

    public int SentenceIndex = 0;

    public bool KeyConfirm = false;
    public bool KeyCancel = false;

    private bool _show = false;
    public bool Show = false;
    public bool Animating = false;

    public float EndTime = -1f;
    public float StartTime = -1f;

    void Update()
    {
        GetInput();
        UpdateAnimator();
        if((!Animating && KeyConfirm) ||
            (!Animating && Time.time > EndTime+StartTime && EndTime > 0f)) TextLoad();
    }

    void GetInput()
    {
        KeyConfirm = KeyCancel = false;
        if (Input.GetKeyDown(KeyCode.Return)) KeyConfirm = true;
        if (Input.GetKeyDown(KeyCode.RightShift)) KeyCancel = true;
    }

    void UpdateAnimator()
    {
        if (Show != _show)
        {
            _show = Show;
            Animating = true;
        }
        if (TransistionAnimator != null)
        {
            TransistionAnimator.SetBool("Show", Show);
        }
    }

    public void AnimationEnd()
    {
        Animating = false;
        if (Show)
        {
            TextLoad();
        }
        else
        {
            if (TargetText != null)
            {
                TargetText.initialized = false;
                TargetText.GetComponent<TMP_Text>().text = " ";
            }
        }
    }

    void TextLoad()
    {
        if (TargetText == null || DialogList == null) return;
        if(SentenceIndex >= DialogList.Dialogs.Count)
        {
            Show = false;
            return;
        }
        EndTime = DialogList.Dialogs[SentenceIndex].AutoNextTime;
        StartTime = Time.time;
        foreach (var dialogEvent in DialogEvents)
        {
            if (dialogEvent.DialogIndex == SentenceIndex)
            {
                dialogEvent.TriggerEvent.Invoke();
            }
        }
        WordData wd = DialogList.Dialogs[SentenceIndex++];
        TargetText.Reset();
        TargetText.Init(wd);
    }

    [Serializable]
    public class DialogEvent
    {
        public int DialogIndex = -1;
        public UnityEvent TriggerEvent = new UnityEvent();

    }
}


