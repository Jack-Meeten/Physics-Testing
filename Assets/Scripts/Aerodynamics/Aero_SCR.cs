using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Aero_SCR : MonoBehaviour
{
    [Header("World Info")]
    float temp = 288.15f;

    [Header("Wing Info")]
    public bool dragOnly, liftOnly;
    [Header("         ")]
    public WingCurves wingCurves;
    public Transform COM;
    public bool drawWings;
    public bool tail;

    [Header("Shape")]
    public float root;
    public float tip;
    public float span;
    public float sweep;
    public Vector3 offset;

    [Header(" ")]
    public bool isControlSurface;
    public bool pitch, yaw, roll;
    [Range(-5, 5)] public float trimPosition;
    public float minTrim = -2;
    public float maxTrim = 2;
    [Header(" ")]
    public float moveSpeed = 90f;
    public float senseFreshHold = 10f;
    [Header(" ")]
    public float maxTorque = 6000;

    Vector3 pointX;
    Vector3 pointY;
    Vector3 pointA;
    Vector3 pointB;
    Rigidbody rb;
    float AOA;
    Vector3 debugLift;

    [Header("Constants")]
    float g = 9.80665f;
    float airMolarMass = 0.02896968f;
    float gasConstant = 8.3144598f;
    float seaLevelAirDensity = 1.2250f;

    [HideInInspector] public bool mirror;

    [HideInInspector] public float area;

    [HideInInspector] public float _lift;

    [HideInInspector] public float _drag;

    [HideInInspector] public float inputVal;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        DrawWings();
        COM.position = (pointA + pointB + pointX + pointY) / 4;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (drawWings)
        {
            DrawWings();
        }
#endif
        area = CalculateArea(root, tip, span);

        Debug.DrawRay(COM.position, rb.GetPointVelocity(transform.position) / 50, Color.red);
        Debug.DrawRay(COM.position, transform.right, Color.green);
        Debug.DrawRay(COM.position, -rb.velocity.normalized * Drag() / 250, Color.yellow);
        Debug.DrawRay(COM.position, debugLift / 700, Color.blue);
    }

    private void FixedUpdate()
    {
        if (liftOnly)
        {
            Vector3 lift = Vector3.zero;
            lift = -Vector3.Cross(rb.velocity, transform.forward).normalized * Lift();

            _lift = lift.magnitude;
            debugLift = lift;

            rb.AddForceAtPosition(lift / 1000, COM.position);
        }

        if (dragOnly)
        {
            Vector3 drag = -rb.velocity.normalized * Drag();

            _drag = drag.magnitude;

            rb.AddForceAtPosition(drag / 1000, COM.position);
        }
        if (!liftOnly && !dragOnly)
        {
            Vector3 lift = Vector3.zero;
            lift = -Vector3.Cross(rb.velocity, transform.forward).normalized * Lift();

            _lift = lift.magnitude;
            debugLift = lift;

            rb.AddForceAtPosition(lift / 1000, COM.position);

            Vector3 drag = -rb.velocity.normalized * Drag();

            _drag = drag.magnitude;

            rb.AddForceAtPosition(drag / 1000, COM.position);
        }

        if (isControlSurface)
        {
            Vector3 rot;
            rot = transform.localEulerAngles;

            float currentDeflection;

            if (inputVal < 0)
            {
                currentDeflection = (inputVal * maxTrim) + trimPosition;
            }
            else
            {
                currentDeflection = (-inputVal * minTrim) + trimPosition;
            }

            if (rb.velocity.sqrMagnitude > senseFreshHold)
            {
                float maxTorqueAtDeflection = rb.velocity.sqrMagnitude * area;
                float maxAvailibleDeflection = Mathf.Asin(maxTorque / maxTorqueAtDeflection) * Mathf.Rad2Deg;

                if (float.IsNaN(maxAvailibleDeflection) == false)
                {
                    float targetAngle = currentDeflection * Mathf.Clamp01(maxAvailibleDeflection);
                    rot.z = Mathf.MoveTowardsAngle(rot.z, targetAngle, moveSpeed * Time.deltaTime);
                    Debug.Log(targetAngle + " : " + name);
                }
            }
            else
            {
                rot.z = Mathf.MoveTowardsAngle(rot.z, currentDeflection, moveSpeed * Time.deltaTime);
            }

            transform.localEulerAngles = rot;
        }
    }

    float Lift()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.GetPointVelocity(transform.position));
        localVel.z = 0;
        float aoa  = GetAOA(localVel);
        //Debug.Log("aoa : " + aoa);
        //Debug.Log("coef : " + wingCurves.lift.Evaluate(aoa));


        return (wingCurves.GetLiftAtAOA(aoa)
            * ((AirDensityCalc(transform.position.y, temp)
            * Mathf.Pow(rb.velocity.magnitude, 2)) / 2)
            * area);
    }

    float Drag()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.GetPointVelocity(transform.position));
        localVel.z = 0;
        float aoa = GetAOA(localVel);

        float dragArea = ((Vector3.Angle(rb.velocity.normalized, transform.up) / 90)) * area;

        //dragArea = ((1 - dragArea) * area);

        if (dragArea < 0)
        {
            dragArea = Mathf.Abs(dragArea);
        }

        return wingCurves.GetDragAtAOA(aoa) * ((AirDensityCalc(transform.position.y, temp) * Mathf.Pow(rb.velocity.magnitude, 2)) / 2) * dragArea;
    }

    float GetAOA(Vector3 worldVelocity)
    {
        //aoa = Vector3.SignedAngle(forwardDir, velocityVehicleSpace, rightDir);
        float aoa = Vector3.Angle(Vector3.right, worldVelocity);
        AOA = aoa;
        return aoa;
    }

    public void Calculate()
    {
        area = CalculateArea(root, tip, span);
        transform.root.GetComponent<Plane_CNTR>().surfacs.Add(this);
    }

    public float CalculateArea(float a, float b, float h)
    {
        return ((a + b) / 2) * h;
    }
    public void Mirror()
    {
        mirror = mirror ? false : true;
    }

    float AirDensityCalc(float altitude, float temp)
    {
        return seaLevelAirDensity * Mathf.Exp(((-g * airMolarMass * altitude) / (gasConstant * temp)));
    }

    void DrawWings()
    {
        if (mirror)
        {
            pointX = new Vector3((transform.position.x + root / 2) + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);
            pointY = new Vector3((transform.position.x - root / 2) + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);
            pointA = new Vector3((transform.position.x + tip / 2) + sweep + offset.x, transform.position.y + offset.y, transform.position.z + span + offset.z);
            pointB = new Vector3((transform.position.x - tip / 2) + sweep + offset.x, transform.position.y + offset.y, transform.position.z + span + offset.z);
        }
        else if (tail)
        {
            pointX = new Vector3((transform.position.x + root / 2) + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);
            pointY = new Vector3((transform.position.x - root / 2) + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);
            pointA = new Vector3((transform.position.x + tip / 2) + sweep + offset.x, transform.position.y + span + offset.y, transform.position.z + offset.z);
            pointB = new Vector3((transform.position.x - tip / 2) + sweep + offset.x, transform.position.y + span + offset.y, transform.position.z + offset.z);
        }
        else
        {
            pointX = new Vector3((transform.position.x + root / 2) + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);
            pointY = new Vector3((transform.position.x - root / 2) + offset.x, transform.position.y + offset.y, transform.position.z + offset.z);
            pointA = new Vector3((transform.position.x + tip / 2) + sweep + offset.x, transform.position.y + offset.y, transform.position.z - span + offset.z);
            pointB = new Vector3((transform.position.x - tip / 2) + sweep + offset.x, transform.position.y + offset.y, transform.position.z - span + offset.z);
        }

        Debug.DrawLine(pointX, pointY);
        Debug.DrawLine(pointX, pointA);
        Debug.DrawLine(pointA, pointB);
        Debug.DrawLine(pointB, pointY);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(COM.position, 0.2f);
        Handles.Label(COM.position, AOA.ToString());
    }
}

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
        if (aero.isControlSurface)
        {
            EditorGUILayout.LabelField("Deflection:", (aero.transform.localEulerAngles.z.ToString()));
        }
    }
}
#endif
