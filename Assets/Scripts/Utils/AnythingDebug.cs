using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnythingDebug : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 0.01f;
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Updatetest()
    {
        transform.rotation = transform.rotation * Quaternion.Euler(0, 0.01f, 0);
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.D)) transform.position += new Vector3(speed, 0, 0);
        if(Input.GetKey(KeyCode.A)) transform.position -= new Vector3(speed, 0, 0);
        if(Input.GetKey(KeyCode.W)) transform.position += new Vector3(0, speed, 0);
        if(Input.GetKey(KeyCode.S)) transform.position -= new Vector3(0, speed, 0);
    }
}
