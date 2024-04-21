using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase : CellModule
{
    private Rigidbody _rb;
    static protected void ClampSpeed(Rigidbody rb, float maxSpeed)
    {
        //use squared as an optimization to avoid expensive sqrt operations
        if (rb.velocity.sqrMagnitude > (maxSpeed * maxSpeed))
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
    protected override void ExtraAwake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        HandleTeamChange(_team);
    }
    public void FixedUpdate()
    {
        if (_rb != null)
        {
            ApplyMovement(_rb, Time.deltaTime);
        }
    }
    public abstract void ApplyMovement(Rigidbody rb, float timePassed);
}
