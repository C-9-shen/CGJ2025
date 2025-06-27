using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogList", menuName = "Dialog/DialogList", order = 1)]
public class DialogList : ScriptableObject
{
    public List<DialogData> Dialogs = new List<DialogData>();
}

