using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase : ScriptableObject
{
    public abstract void ApplyMovement(Rigidbody rb, float timePassed);
    static protected void ClampSpeed(Rigidbody rb, float maxSpeed)
    {
        //use squared as an optimization to avoid expensive sqrt operations
        if (rb.velocity.sqrMagnitude > (maxSpeed * maxSpeed))
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

    }
}
