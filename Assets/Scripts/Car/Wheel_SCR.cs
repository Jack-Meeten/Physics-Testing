using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class Wheel_SCR : MonoBehaviour
{
    private Rigidbody rB;

    public bool wheelFrontLeft, wheelFrontRight, wheelRearLeft, wheelRearRight;
    public PlayerInput pI;

    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;

    private float minLength;
    private float maxLength;
    private float springLength;
    private float lastLength;
    private float springForce;
    private float damperForce;
    private float springVelocity;

    private Vector3 suspensionForce;
    private float wheelAngle;
    private Vector3 wheelVelLS;
    private float fX;
    private float fY;

    [Header("Wheel")]
    public bool powerd = false;
    public float wheelRadius;
    public float steerAngle;
    public float steerTime;
    public float wheelForce;
    public float breakingForce;

    [Header("Friction")]
    public AnimationCurve rollingResistanceCoef;

    private Vector3 frictionForce;
    private Vector3 movementDir;
    private Vector3 movementDirLocal;
    private Vector3 slipAngle;
    private Vector3 currentBreakingForce;

    void Start()
    {
        rB = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }

    void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
        Debug.DrawRay(transform.position, -transform.up * (springLength + wheelRadius), Color.green);
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            //calculate suspension forces
            lastLength = springLength;
            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);
            springVelocity = (lastLength - springLength) / Time.deltaTime;
            springForce = springStiffness * (restLength - springLength);
            damperForce = damperStiffness * springVelocity;

            suspensionForce = (springForce + damperForce) * transform.up;

            //set tire mesh pos
            Vector3 tirePos = transform.GetChild(0).position;
            tirePos.y = hit.point.y + wheelRadius;
            transform.GetChild(0).position = tirePos;

            //apply movement
            if (powerd)
            {
                fX = pI.actions["FB"].ReadValue<float>() * wheelForce;
            }
            if (pI.actions["Break"].ReadValue<float>() == 1)
            {
                currentBreakingForce.z = Mathf.Clamp(rB.velocity.normalized.z * 0.9f * (breakingForce * (rB.velocity.magnitude / 2)), 0, 4500);// * (breakingEXP / 5)
                Debug.Log(currentBreakingForce.z);
            }
            else
            {
                currentBreakingForce.z = 0;
            }

            wheelVelLS = transform.InverseTransformDirection(rB.GetPointVelocity(hit.point));

            fY = wheelVelLS.x * springForce;

            //friction
            movementDir = rB.transform.eulerAngles;
            movementDirLocal = transform.eulerAngles;
            slipAngle = movementDir - movementDirLocal;

            frictionForce.z = rollingResistanceCoef.Evaluate(rB.velocity.magnitude) * (rB.mass * Physics.gravity.y) * slipAngle.y;

            rB.AddForceAtPosition(suspensionForce + (fX * transform.forward) + (fY * -transform.right) + -frictionForce + -currentBreakingForce, hit.point);
            //Debug.Log("total force" + (suspensionForce + (fX * transform.forward) + (fY * -transform.right) + -frictionForce + -currentBreakingForce));
        }
    }

    private void OnDrawGizmosSelected()
    {
        GUI.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - restLength, transform.position.z), wheelRadius);
        Handles.Label(transform.position, (suspensionForce + (fX * transform.forward) + (fY * -transform.right) + -frictionForce + currentBreakingForce).ToString());
    }
}
