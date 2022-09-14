using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Engine_SCR : MonoBehaviour
{
    [Header("Engine Specs")]
    public float thrust;

    [HideInInspector] public float input;

    [HideInInspector] public float pec;
    Rigidbody rB;

    void Start()
    {
        rB = transform.root.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Throttle();
        Debug.DrawRay(transform.position, transform.right * 1, Color.cyan);
    }
    void Throttle()
    {
        pec += 0.1f * input;
        pec = Mathf.Clamp(pec, 0, 1);
        float actual = pec * thrust;
        rB.AddForce(transform.right * actual);
    }
    public void Calculate()
    {
        transform.root.GetComponent<Plane_CNTR>().engines.Add(this);
    }
#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(Engine_SCR))]
    public class Custom : Editor
    {
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();

            Engine_SCR eN = (Engine_SCR)target;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Specs", EditorStyles.boldLabel);
            eN.thrust = EditorGUILayout.FloatField(eN.thrust);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Add to Controller"))
            {
                eN.Calculate();
            }
            EditorGUILayout.EndVertical();
        }
    }
#endif
}
