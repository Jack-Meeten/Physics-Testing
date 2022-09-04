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
    public WingCurves wingCurves;
    public Transform COM;
    public bool drawWings;
    public bool tail;

    [Header("Shape")]
    public float root;
    public float tip;
    public float span;
    public float sweep;

    Vector3 pointX;
    Vector3 pointY;
    Vector3 pointA;
    Vector3 pointB;
    Rigidbody rb;
    float AOA;

    [Header("Constants")]
    float g = 9.80665f;
    float airMolarMass = 0.02896968f;
    float gasConstant = 8.3144598f;
    float seaLevelAirDensity = 1.2250f;

    [HideInInspector] public bool mirror;

    [HideInInspector] public float area;

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
    }

    private void FixedUpdate()
    {
        Vector3 lift = Vector3.zero;
        //lift = rb.velocity.normalized * Lift();
        lift = -Vector3.Cross(rb.velocity.normalized, transform.InverseTransformDirection(transform.forward)) * Lift();
        Debug.DrawRay(COM.position, lift / 100, Color.blue);
        Debug.DrawRay(COM.position, rb.velocity.normalized, Color.red);
        Debug.DrawRay(COM.position, transform.InverseTransformDirection(transform.right), Color.green);


        rb.AddForceAtPosition(lift / 1000, COM.position);
        Debug.Log("Lift : " + lift);
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

    float GetAOA(Vector3 worldVelocity)
    {
        float aoa;

        //aoa = Vector3.SignedAngle(forwardDir, velocityVehicleSpace, rightDir);
        aoa = Vector3.Angle(Vector3.right, transform.InverseTransformDirection(worldVelocity));
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
            pointX = new Vector3(transform.position.x + root / 2, transform.position.y, transform.position.z);
            pointY = new Vector3(transform.position.x - root / 2, transform.position.y, transform.position.z);
            pointA = new Vector3((transform.position.x + tip / 2) + sweep, transform.position.y, transform.position.z + span);
            pointB = new Vector3((transform.position.x - tip / 2) + sweep, transform.position.y, transform.position.z + span);
        }
        else if (tail)
        {
            pointX = new Vector3(transform.position.x + root / 2, transform.position.y, transform.position.z);
            pointY = new Vector3(transform.position.x - root / 2, transform.position.y, transform.position.z);
            pointA = new Vector3((transform.position.x + tip / 2) + sweep, transform.position.y + span, transform.position.z);
            pointB = new Vector3((transform.position.x - tip / 2) + sweep, transform.position.y + span, transform.position.z);
        }
        else
        {
            pointX = new Vector3(transform.position.x + root / 2, transform.position.y, transform.position.z);
            pointY = new Vector3(transform.position.x - root / 2, transform.position.y, transform.position.z);
            pointA = new Vector3((transform.position.x + tip / 2) + sweep, transform.position.y, transform.position.z - span);
            pointB = new Vector3((transform.position.x - tip / 2) + sweep, transform.position.y, transform.position.z - span);
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
