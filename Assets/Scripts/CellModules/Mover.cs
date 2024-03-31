using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeamTracker))]
public class Mover : CellModule {
    //private float _forceMagnitude;
    //private float _sqrMinimumVelocity;
    private float _sqrMaximumVelocity;

    public Vector3 TargetDirection = Vector3.zero;
    public float MaxSpeed = 4;
    private float _frictionDynamicCoefficientWithGround = 0f;
    public float BaseAcceleration = 4;
    private float _propulsiveForceMagnitude;

    private Rigidbody _rb;

    protected override void ExtraAwake() {
        _rb = gameObject.GetComponent<Rigidbody>();
        if (_rb == null) {
            Debug.LogError("Moving body doesn't have a rigidbody attached!");
        }
    }

    private void Start() {
        // The player's mass will increase and decrease as they get new parts.
        var baseMass = _rb.mass;
        Utils.LogOncePerSecond($"{gameObject} base mass: {_rb.mass}");
        var frictionalForceMagnitude = _frictionDynamicCoefficientWithGround * baseMass;
        _propulsiveForceMagnitude = (baseMass * BaseAcceleration) + frictionalForceMagnitude;
        _sqrMaximumVelocity = MaxSpeed * MaxSpeed;
    }

    public void FixedUpdate() {
        // Set the velocity to 0 manually at low velocities to avoid weird
        // slow persistent movement bug.
        if (_rb.velocity.sqrMagnitude <= .000001) {
            _rb.velocity = Vector3.zero;
        } else {
            // Friction
            Vector3 velocityDirection = _rb.velocity.normalized;
            Vector3 frictionalForce = -velocityDirection * _rb.mass * _frictionDynamicCoefficientWithGround;
            _rb.AddForce(frictionalForce);
        }
        //_forceMagnitude = (PropulsiveForce + _frictionDynamicCoefficientWithGround) * _rb.mass;

        if (TargetDirection == Vector3.zero) {
            return;
        } else {
            // Add movement
            Vector3 propulsiveForce = TargetDirection * _propulsiveForceMagnitude;
            _rb.AddForce(propulsiveForce);
        }

        // Slow down the body if it is moving too fast.
        if (_rb.velocity.sqrMagnitude > _sqrMaximumVelocity) {
            _rb.velocity = _rb.velocity.normalized * MaxSpeed;
        }
    }

    public void StopMoving() {
        TargetDirection = Vector3.zero;
    }

    protected override void HandleTeamChange(Team newTeam) {
        if (newTeam == Team.Neutral) StopMoving();
    }
}
