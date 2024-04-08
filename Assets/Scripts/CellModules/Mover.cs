using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {
    private float _sqrMaximumVelocity;

    public Vector3 TargetDirection = Vector3.zero;
    public float MaxSpeed = 4;
    public float BaseAcceleration = 4;
    private float _propulsiveForceMagnitude;

    private Rigidbody _rb;

    private void Awake() {
        _rb = gameObject.GetComponent<Rigidbody>();
        if (_rb == null) {
            Debug.LogError($"Mover on {gameObject} doesn't have a rigidbody attached!");
        }
    }

    private void Start() {
        // The player's mass will increase and decrease as they get new parts.
        var baseMass = _rb.mass;
        _propulsiveForceMagnitude = (baseMass * BaseAcceleration);
        _sqrMaximumVelocity = MaxSpeed * MaxSpeed;
    }

    public void FixedUpdate() {
        if (TargetDirection == Vector3.zero) {
            return;
        }
        if (_rb == null) {
            Utils.LogOncePerSecond($"Tried moving {gameObject} with no RigidBody.");
        }

        // Add movement
        Vector3 propulsiveForce = TargetDirection * _propulsiveForceMagnitude;
        _rb.AddForce(propulsiveForce);

        // Slow down the body if it is moving too fast.
        if (_rb.velocity.sqrMagnitude > _sqrMaximumVelocity) {
            _rb.velocity = _rb.velocity.normalized * MaxSpeed;
        }
    }

    public void StopMoving() {
        TargetDirection = Vector3.zero;
    }
}
