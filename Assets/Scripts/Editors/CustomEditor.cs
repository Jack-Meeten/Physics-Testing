using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(Aero_SCR))]
public class CustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Aero_SCR aero = (Aero_SCR)target;

        if (GUILayout.Button("Flip Direction"))
        {
            aero.Mirror();
        }
        if (GUILayout.Button("Calculate Paramaters"))
        {
            aero.Calculate();
        }
        EditorGUILayout.LabelField("Area:", (aero.area.ToString() + " M^2"));
        EditorGUILayout.LabelField("Flipped:", (aero.mirror.ToString()));
        EditorGUILayout.LabelField("Lift:", (aero._lift.ToString()));
        EditorGUILayout.LabelField("Drag:", (aero._drag.ToString()));
    }
}
#endif