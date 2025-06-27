using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// using Microsoft.Unity.VisualStudio.Editor;

[CreateAssetMenu(fileName = "WordData", menuName = "Words/WordData", order = 1)]
public class WordData : ScriptableObject
{
    public string ID = null;
    [TextArea(5, 10)]
    public string WordContent = null;
    public float WordSize = 16;
    public bool AllowFontAutoSize = false;
    public Vector2 BoxSize = new Vector2(-1, -1);
}

