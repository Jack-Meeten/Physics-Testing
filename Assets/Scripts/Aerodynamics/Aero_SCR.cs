using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Aero_SCR : MonoBehaviour
{
    [Header("World Info")]
    public float temp = 288.15f;

    [Header("Wing Info")]
    public bool dragOnly;
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

        Debug.DrawRay(COM.position, transform.InverseTransformDirection(rb.GetPointVelocity(transform.position)) / 50, Color.red);
        Debug.DrawRay(COM.position, transform.right, Color.green);
        Debug.DrawRay(COM.position, -rb.velocity.normalized * Drag() / 250, Color.yellow);
        Debug.DrawRay(COM.position, debugLift / 700, Color.blue);
    }

    private void FixedUpdate()
    {
        if (!dragOnly)
        {
            Vector3 lift = Vector3.zero;
            lift = -Vector3.Cross(rb.velocity, transform.forward).normalized * Lift();

            _lift = lift.magnitude;
            debugLift = lift;

            rb.AddForceAtPosition(lift / 1000, COM.position);
        }


        Vector3 drag = -rb.velocity.normalized * Drag();

        _drag = drag.magnitude;

        Debug.Log(-rb.velocity.normalized); 


        rb.AddForceAtPosition(drag / 1000, COM.position);

        //Debug.Log("Lift : " + lift);
        //Debug.Log("Raw Lift : " + Lift());
        //Debug.Log(-Vector3.Cross(rb.velocity.normalized, transform.InverseTransformDirection(transform.forward)));
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
#endif
}
