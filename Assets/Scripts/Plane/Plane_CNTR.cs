using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;


public class Plane_CNTR : MonoBehaviour
{
    [Header("Control")]

    public PlayerInput pI;

    [Header("Engines")]
    public List<Engine_SCR> engines = new List<Engine_SCR>();

    [Header("Surfaces")]

    [Range(-5, 5)]public float trimPosition;
    public float minTrim = -2;
    public float minLimit = -10;
    public float maxTrim = 2;
    public float maxLimit = 10;
    public List<Aero_SCR> surfacs = new List<Aero_SCR>();

    [Header("UI")]
    public TextMeshProUGUI speed;
    public TextMeshProUGUI alt;
    public TextMeshProUGUI throttle;


    Rigidbody rB;
    void Start()
    {
        rB = transform.root.GetComponent<Rigidbody>();
    }

    public void Update()
    {
        UIUpdate();

        float pitchInput = pI.actions["Pitch"].ReadValue<float>();
        float rollInput = pI.actions["Roll"].ReadValue<float>();
        float thrustInput = pI.actions["Throttle"].ReadValue<float>();

        for (int i = 0; i < surfacs.Count; i++)
        {
            if (surfacs[i].pitch)
            {
                surfacs[i].inputVal = -pitchInput;
            }
            else if (surfacs[i].roll)
            {
                if (surfacs[i].mirror)
                {
                    surfacs[i].inputVal = -rollInput;
                }
                else
                {
                    surfacs[i].inputVal = rollInput;
                }
            }
        }
        for (int i = 0; i < engines.Count; i++)
        {
            engines[i].input = -thrustInput;
        }
    }

    void UIUpdate()
    {
        speed.text = "Vel : " + rB.velocity.magnitude.ToString("0.00");
        alt.text = "Alt : " + transform.position.y.ToString("0.00");
        throttle.text = "Thr : " + engines[0].pec * 100 + "%";
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(Plane_CNTR))]
public class Custom : Editor
{
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        EditorGUILayout.BeginVertical("box");

        Plane_CNTR cNTR = (Plane_CNTR)target;

        GUILayout.Label("Control", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Player Input");
        EditorGUILayout.ObjectField(cNTR.pI, typeof(PlayerInput), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Surfaces", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("Trim");
        EditorGUILayout.Slider(cNTR.trimPosition, -5, 5);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Min Trim");
        cNTR.minTrim = EditorGUILayout.FloatField(cNTR.minTrim);
        GUILayout.Label("Max Trim ");
        cNTR.maxTrim = EditorGUILayout.FloatField(cNTR.maxTrim);
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Surfaces");
        var list = cNTR.surfacs;
        int newCount = Mathf.Max(0, EditorGUILayout.IntField("size", list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(null);

        for (int i = 0; i < list.Count; i++)
        {
            list[i] = (Aero_SCR)EditorGUILayout.ObjectField(list[i], typeof(Aero_SCR), true);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("Engines");
        var listEn = cNTR.engines;
        int newCountEn = Mathf.Max(0, EditorGUILayout.IntField("size", listEn.Count));
        while (newCountEn < listEn.Count)
            listEn.RemoveAt(listEn.Count - 1);
        while (newCountEn > listEn.Count)
            listEn.Add(null);

        for (int i = 0; i < listEn.Count; i++)
        {
            listEn[i] = (Engine_SCR)EditorGUILayout.ObjectField(listEn[i], typeof(Engine_SCR), true);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("UI");
        cNTR.speed = (TextMeshProUGUI)EditorGUILayout.ObjectField(cNTR.speed, typeof(TextMeshProUGUI), true);
        cNTR.alt = (TextMeshProUGUI)EditorGUILayout.ObjectField(cNTR.alt, typeof(TextMeshProUGUI), true);
        cNTR.throttle = (TextMeshProUGUI)EditorGUILayout.ObjectField(cNTR.throttle, typeof(TextMeshProUGUI), true);

        EditorGUILayout.EndVertical();

    }
}
#endif

