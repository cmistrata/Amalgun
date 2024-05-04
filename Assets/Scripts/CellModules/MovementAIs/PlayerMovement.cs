using UnityEngine;

public class PlayerMovement : DirectionForceMovementBase {
    private const float TORQUE = 5000f;
    private float _propulsiveForceMagnitude;

    public override void ApplyMovement(Rigidbody rb, float timePassed) {
        TargetDirection = (new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))).normalized;

        float clockwiseRotationInput = Input.GetAxis("Rotate Clockwise");
        rb.AddTorque(clockwiseRotationInput * TORQUE * Vector3.up);

        ApplyForce(rb);
    }


    protected override void ExtraAwake() {
        base.ExtraAwake();
        _propulsiveForceMagnitude = _rb.mass * Acceleration;
    }

    protected override float DetermineForceMagnitude() {
        return _propulsiveForceMagnitude;
    }
}
