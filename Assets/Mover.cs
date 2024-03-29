using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeamTracker))]
public class Mover : MonoBehaviour {
    private float _forceMagnitude;
    //private float _sqrMinimumVelocity;
    private float _sqrMaximumVelocity;

    public Vector3 TargetDirection = Vector3.zero;
    public float MaxSpeed = 4;
    public float FrictionDynamicCoefficientWithGround = 25;
    public float Acceleration = 20;

    private Rigidbody _rb;
    private TeamTracker _teamTracker;

    void Awake() {
        _rb = gameObject.GetComponent<Rigidbody>();
        _teamTracker = GetComponent<TeamTracker>();
        _teamTracker.ChangeTeamEvent += HandleChangeTeam;
        if (_rb == null) {
            Debug.LogError("Moving body doesn't have a rigidbody attached!");
        }
        _forceMagnitude = (Acceleration + FrictionDynamicCoefficientWithGround) * _rb.mass;
        _sqrMaximumVelocity = MaxSpeed * MaxSpeed;
    }

    public void FixedUpdate() {
        // Set the velocity to 0 manually at low velocities to avoid weird
        // slow persistent movement bug.
        if (_rb.velocity.sqrMagnitude <= .1) {
            _rb.velocity = Vector3.zero;
        } else {
            // Friction
            Vector3 velocityDirection = _rb.velocity.normalized;
            Vector3 frictionalForce = -velocityDirection * _rb.mass * FrictionDynamicCoefficientWithGround;
            _rb.AddForce(frictionalForce);
        }
        _forceMagnitude = (Acceleration + FrictionDynamicCoefficientWithGround) * _rb.mass;

        if (TargetDirection == Vector3.zero) {
            return;
        } else {
            // Add movement
            Vector3 propulsiveForce = TargetDirection * _forceMagnitude;
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

    public void HandleChangeTeam(Team newTeam) {
        if (newTeam == Team.Neutral) StopMoving();
    }
}
