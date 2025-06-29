using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedLengthOutLineBox : MonoBehaviour
{
    public GameObject ParentObject;
    public float OutlineWidth;
    public Color color = Color.white;
    public GameObject InnerPart;

    void Update()
    {
        if(ParentObject == null || InnerPart == null)
            return;
        if(ParentObject.transform.lossyScale.x == 0 ||
            ParentObject.transform.lossyScale.y == 0 ||
            ParentObject.transform.lossyScale.z == 0)
            return;
        Vector3 scale = new Vector3(
            transform.lossyScale.x / ParentObject.transform.lossyScale.x,
            transform.lossyScale.y / ParentObject.transform.lossyScale.y,
            transform.lossyScale.z / ParentObject.transform.lossyScale.z
        );
        scale = scale - Vector3.one * OutlineWidth;
        if(transform.lossyScale.x ==0 || 
            transform.lossyScale.y == 0 || 
            transform.lossyScale.z == 0)
            return;
        InnerPart.transform.localScale = new Vector3(
            ParentObject.transform.lossyScale.x * scale.x/transform.lossyScale.x,
            ParentObject.transform.lossyScale.y * scale.y/transform.lossyScale.y,
            ParentObject.transform.lossyScale.z * scale.z/transform.lossyScale.z
        );
        InnerPart.GetComponent<SpriteRenderer>().color = color;
    }

    public void UpdateInEditor()
    {
        if(Application.isPlaying) return;
        Update();
    }
}
