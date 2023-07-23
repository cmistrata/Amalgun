using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GroundFrictionApplier : MonoBehaviour
{
    private Rigidbody2D _rb2d;

    public float DynamicCoefficientOfFrictionWithGround = 25;

    private void Awake() {
        _rb2d = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        Vector2 frictionalForce = DynamicCoefficientOfFrictionWithGround * _rb2d.mass * -(_rb2d.velocity.normalized);
        _rb2d.AddForce(frictionalForce);

        //// Add movement
        //Vector2 totalForce = new Vector2();
        //totalForce = TargetDirection * ForceMagnitude;
        //// Slow down based on velocity and mass
        //float frictionMagnitude = Rb2d.mass * FrictionRatio;
        //totalForce += frictionMagnitude * Rb2d.velocity.magnitude * -Rb2d.velocity;

        //Rb2d.AddForce(totalForce);
        //if (Rb2d.velocity.magnitude < .5 && TargetDirection.magnitude == 0) {
        //    Rb2d.velocity = new Vector2(0, 0);
        //}
    }
}
