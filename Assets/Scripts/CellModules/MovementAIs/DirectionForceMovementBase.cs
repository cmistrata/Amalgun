using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DirectionForceMovementBase : MovementBase
{
    [SerializeField]
    protected float Acceleration = 4f;
    [SerializeField]
    protected float MaxSpeed = -1f;
    public Vector3 TargetDirection = Vector3.zero;

    protected virtual float DetermineForceMagnitude() {
        return (_rb.mass * Acceleration);
    }

    protected virtual void ApplyForce(Rigidbody rb)
    {
        if (TargetDirection == Vector3.zero) return;

        Vector3 force = DetermineForceMagnitude() * TargetDirection;
        rb.AddForce(force);
        if (MaxSpeed > 0) {
            MovementBase.ClampSpeed(rb, MaxSpeed);
        }
    }
}
