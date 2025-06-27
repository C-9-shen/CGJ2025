using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleChooseButton : MonoBehaviour
{
    public List<SimpleButton> buttons = new List<SimpleButton>();

    public int ActiveIndex = -1;

    public void SetActiveIndex(int index)
    {
        if (index < 0 || index >= buttons.Count) return;

        if (ActiveIndex >= 0 && ActiveIndex < buttons.Count)
        {
            buttons[ActiveIndex].SetActivating(false);
        }
        ActiveIndex = index;
        buttons[ActiveIndex].SetActivating(true);
    }
}
