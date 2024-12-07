using UnityEngine;

public abstract class MovementBase : CellModule {
    protected Rigidbody _rb;
    static protected void ClampSpeed(Rigidbody rb, float maxSpeed) {
        //use squared as an optimization to avoid expensive sqrt operations
        if (rb.linearVelocity.sqrMagnitude > (maxSpeed * maxSpeed)) {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    protected override void ExtraAwake() {
        _rb = gameObject.GetComponent<Rigidbody>();
        HandleStateChange(_state);
    }
    public void FixedUpdate() {
        if (_rb != null) {
            ApplyMovement(_rb, Time.deltaTime);
        }
    }
    public abstract void ApplyMovement(Rigidbody rb, float timePassed);
}
