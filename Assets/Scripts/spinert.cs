using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spinert : Bullet
{
    public float speed=1.5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0, speed* Time.deltaTime, 0);
    }
}
