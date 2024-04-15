using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DirectionForceMovementBase : BaseMovement
{
    [SerializeField]
    protected float Acceleration = 4f;
    [SerializeField]
    protected float MaxSpeed = 4f;
    protected Vector3 TargetDirection = Vector3.zero;

    protected void ApplyForce(Rigidbody rb)
    {
        if (TargetDirection == Vector3.zero) return;

        Vector3 force = (rb.mass * Acceleration) * TargetDirection;
        rb.AddForce(force);
        IMovement.ClampSpeed(rb, MaxSpeed);
    }
}
