using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;


public class Plane_CNTR : MonoBehaviour
{
    [Header("Control")]

    public PlayerInput pI;

    [Header("Engines")]

    [Header("Surfaces")]

    [Range(-5, 5)]public float trimPosition;


    private Rigidbody rB;
    void Start()
    {
        rB = transform.root.GetComponent<Rigidbody>();
    }


}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(Plane_CNTR))]
public class Custom : Editor
{
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        Plane_CNTR cNTR = (Plane_CNTR)target;

        GUILayout.Label("Control", EditorStyles.boldLabel);
    }
}
#endif

