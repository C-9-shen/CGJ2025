using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogData", menuName = "Dialog/DialogData", order = 1)]
public class DialogData : WordData
{
    public float AutoNextTime = -1f;
}
