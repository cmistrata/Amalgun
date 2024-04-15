using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovement", menuName = "Player Movement")]
public class PlayerMovement : DirectionForceMovementBase
{
    private const float TORQUE = 5000f;
    public override void ApplyMovement(Rigidbody rb, float timePassed)
    {
        TargetDirection = (new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))).normalized;

        float clockwiseRotationInput = Input.GetAxis("Rotate Clockwise");
        rb.AddTorque(clockwiseRotationInput * TORQUE * Vector3.up);

        ApplyForce(rb);
    }
}
