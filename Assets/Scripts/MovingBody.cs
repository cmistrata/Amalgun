using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBody : MonoBehaviour
{
    private float _forceMagnitude;
    //private float _sqrMinimumVelocity;
    private float _sqrMaximumVelocity;

    public Vector2 TargetDirection = Vector2.zero;
    public float MaxSpeed = 4;
    public float FrictionDynamicCoefficientWithGround = 25;
    public float Acceleration = 20;

    private Rigidbody2D _rb;

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        if (_rb == null ) {
            Debug.LogError("Moving body doesn't have a rigidbody attached!");
        }
        _forceMagnitude = (Acceleration + FrictionDynamicCoefficientWithGround) * _rb.mass;
        //_sqrMinimumVelocity = MinimumVelocity * MinimumVelocity;
        _sqrMaximumVelocity = MaxSpeed * MaxSpeed;
    }

    public void FixedUpdate()
    {
        _forceMagnitude = (Acceleration + FrictionDynamicCoefficientWithGround) * _rb.mass;

        // Friction
        Vector2 velocityDirection = _rb.velocity.normalized;
        Vector2 frictionalForce = -velocityDirection * _rb.mass * FrictionDynamicCoefficientWithGround;
        _rb.AddForce(frictionalForce);

        if (TargetDirection == Vector2.zero) {
            //if (_rb.velocity.sqrMagnitude < _sqrMinimumVelocity) {
            //    _rb.velocity = Vector2.zero;
            //}
            return;
        } else {
            // Add movement
            Vector2 propulsiveForce = TargetDirection * _forceMagnitude;
            _rb.AddForce(propulsiveForce);
        }

        // Slow down the body if it is moving too fast.
        if (_rb.velocity.sqrMagnitude > _sqrMaximumVelocity) {
            _rb.velocity = _rb.velocity.normalized * MaxSpeed;
        }
    }
}
