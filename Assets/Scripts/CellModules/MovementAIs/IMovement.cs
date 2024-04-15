using UnityEngine;

public interface IMovement
{
    static void ClampSpeed(Rigidbody rb, float maxSpeed)
    {
        //use squared as an optimization to avoid expensive sqrt operations
        if (rb.velocity.sqrMagnitude > (maxSpeed * maxSpeed))
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

    }
    void ApplyMovement(Rigidbody rb, float timePassed);
}
