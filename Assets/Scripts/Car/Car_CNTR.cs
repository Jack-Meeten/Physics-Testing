using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Car_CNTR : MonoBehaviour
{
    public Wheel_SCR[] wheels;

    [Header("CarSpecs")]
    public float wheelBase;//in M
    public float rearTrack;//in M
    public float turnRadius; //in M
    public float topSpeed;

    [Header("Friction")]
    public AnimationCurve dragCurve;
    public float dragArea;
    public float dragCoef;

    [Header("Inputs")]
    public float steerInput;
    public PlayerInput pI;
    [Header(" ")]
    public TextMeshProUGUI speedtext;

    private float ackermannAngleLeft;
    private float ackermannAngleRight;
    private Vector3 dragForce;
    private Rigidbody rB;

    void Start()
    {
        wheelBase = wheels[0].transform.position.z - wheels[1].transform.position.z;
        rearTrack = wheels[2].transform.position.x - wheels[0].transform.position.x;
        rB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        steerInput = pI.actions["LR"].ReadValue<float>();

        if (steerInput > 0)//turning right
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
        }
        else if (steerInput < 0)//turning left
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }

        foreach (Wheel_SCR w in wheels)
        {
            if (w.wheelFrontLeft) w.steerAngle = ackermannAngleLeft;
            if (w.wheelFrontRight) w.steerAngle = ackermannAngleRight;
        }

        speedtext.text = GetComponent<Rigidbody>().velocity.magnitude.ToString("0.00");
    }
    private void FixedUpdate()
    {
        //drag
        dragForce.z = dragCurve.Evaluate(rB.velocity.magnitude / topSpeed) * dragArea * dragCoef;
        rB.AddForce(-dragForce * 1500);
    }
}
