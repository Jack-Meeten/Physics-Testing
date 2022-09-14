﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset;

    private void LateUpdate()
    {
        transform.position = target.transform.position + offset;
        transform.LookAt(transform.position);
    }
}
