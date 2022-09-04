using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Force : MonoBehaviour
{
    public Vector3 force;
    public bool constant;
    void Start()
    {
        transform.root.GetComponent<Rigidbody>().AddForce(force);
    }
    private void FixedUpdate()
    {
        if (constant)
        {
            transform.root.GetComponent<Rigidbody>().AddForce(force);
        }
    }
}
