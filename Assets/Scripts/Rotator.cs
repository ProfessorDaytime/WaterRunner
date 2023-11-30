using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float ySpeed = 1;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0f,ySpeed, 0f,Space.World);
    }
}
