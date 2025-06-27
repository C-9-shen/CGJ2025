using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Link2Data", menuName = "Words/Link2Data", order = 1)]
public class Link2Data : ScriptableObject
{
    public List<WordData> wordBoxDataList = new List<WordData>();
}
