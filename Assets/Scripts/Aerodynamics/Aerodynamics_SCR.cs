using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aerodynamics_SCR : MonoBehaviour
{
    [Header("Constants")]
    float g = 9.80665f;
    float airMolarMass = 0.02896968f;
    float gasConstant = 8.3144598f;
    float seaLevelAirDensity = 1.2250f;

    Vector3 last;
    float lastDistanceMoved;
    float acceleration;

    public Vector3[] verts;
    public int[] indicies;
    public Vector3[] normals;
    public List<Vector3> center = new List<Vector3>();
    public List<Vector3> faceNormals = new List<Vector3>();
    public List<float> areas = new List<float>();

    float exposedSA;
    Vector3 averagePos;

    Rigidbody rB;

    public float temp = 288.15f;

    [Header("Drag")]
    public float dragCoef;
    
    private void Start()
    {
        rB = transform.root.GetComponent<Rigidbody>();

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        verts = mesh.vertices;
        indicies = mesh.triangles;
        normals = mesh.normals;

        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            Vector3 P1 = verts[indicies[i++]];
            Vector3 P2 = verts[indicies[i++]];
            Vector3 P3 = verts[indicies[i++]];
            //Debug.Log(i);
            areas.Add(CalculateAreaOfTrinagle(P1, P2, P3));

            center.Add((P1 + P2 + P3) / 3);
        }
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            Vector3 n1 = normals[indicies[i++]];
            Vector3 n2 = normals[indicies[i++]];
            Vector3 n3 = normals[indicies[i++]];

            faceNormals.Add((n1 + n2 + n3) / 3);
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log(AccelerationCalculation());

        Vector3 applyPos;
        Vector3 drag;


        drag = -rB.velocity * Drag(out applyPos);
        rB.AddForceAtPosition(drag, applyPos);
    }

    float Drag(out Vector3 applyPos)
    {
        exposedSA = 0;
        averagePos = Vector3.zero;
        float pointCount = 0;

        for (int i = 0; i < faceNormals.Count; i++)
        {
            Debug.DrawRay(transform.TransformPoint(center[i]), faceNormals[i], Color.blue);

            float angleFromNormal = (transform.InverseTransformDirection(rB.velocity) - faceNormals[i]).magnitude;
            Debug.Log(angleFromNormal + "  " + i);

            if (angleFromNormal < 90)
            {
                float pecentage = angleFromNormal / 90;

                exposedSA += areas[i] * pecentage;
                averagePos += center[i];
                pointCount++;
                //Debug.Log("Area = " + exposedSA);
            }
        }

        float drag = 
            dragCoef * 
            ((AirDensityCalc(transform.position.y, temp) * Mathf.Pow(rB.velocity.magnitude, 2)) / 2)
            * exposedSA;

        applyPos = averagePos / pointCount;

        return drag;
    }

    float AirDensityCalc(float altitude, float temp)
    {
        return seaLevelAirDensity * Mathf.Exp(((-g * airMolarMass * altitude) / (gasConstant * temp)));
    }

    float AccelerationCalculation()
    {
        float distanceMoved = Vector3.Distance(last, transform.position);
        distanceMoved *= Time.deltaTime;
        acceleration = distanceMoved - lastDistanceMoved;
        lastDistanceMoved = distanceMoved;
        last = transform.position;
        return acceleration;
    }
    float CalculateAreaOfTrinagle(Vector3 n1, Vector3 n2, Vector3 n3)
    {
        float res = Mathf.Pow(((n2.x * n1.y) - (n3.x * n1.y) - (n1.x * n2.y) + (n3.x * n2.y) + (n1.x * n3.y) - (n2.x * n3.y)), 2.0f);
        res += Mathf.Pow(((n2.x * n1.z) - (n3.x * n1.z) - (n1.x * n2.z) + (n3.x * n2.z) + (n1.x * n3.z) - (n2.x * n3.z)), 2.0f);
        res += Mathf.Pow(((n2.y * n1.z) - (n3.y * n1.z) - (n1.y * n2.z) + (n3.y * n2.z) + (n1.y * n3.z) - (n2.y * n3.z)), 2.0f);
        return Mathf.Sqrt(res) * 0.5f;
    }
}
