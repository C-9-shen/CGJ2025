using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnythingDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Updatetest()
    {
        transform.rotation = transform.rotation * Quaternion.Euler(0, 0.01f, 0);
    }
}
