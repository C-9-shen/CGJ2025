using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDialog : MonoBehaviour
{
    public static GameObject _instance;
    public static DialogBox Instance
    {
        get
        {
            if (_instance == null) {
                _instance = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Windows/DialogBox"));
                DontDestroyOnLoad(_instance);
            }
            return _instance.GetComponent<DialogBox>();
        }
    }

    public Canvas TargetCanvas;
    public DialogList DialogList;
    public float ShowHeight = 0f;

    public List<DialogBox.DialogEvent> DialogEvents = new List<DialogBox.DialogEvent>();

    [ContextMenu("Show Dialog")]
    public void Show()
    {
        if (TargetCanvas == null)
        {
            Debug.LogError("TargetCanvas is not set in ShowDialog.");
            return;
        }

        if (DialogList == null || DialogList.Dialogs.Count == 0)
        {
            Debug.LogError("DialogList is empty or not set in ShowDialog.");
            return;
        }

        DialogBox dialogBox = Instance;
        dialogBox.SentenceIndex = 0;
        dialogBox.DialogList = DialogList;
        dialogBox.DialogEvents = DialogEvents;
        if(dialogBox.Showing) dialogBox.TextLoad();
        dialogBox.Show = true;
        

        RectTransform rectTransform = _instance.GetComponent<RectTransform>();
        rectTransform.SetParent(TargetCanvas.transform, false);
        rectTransform.anchoredPosition = new Vector2(0f, ShowHeight);
    }
}
