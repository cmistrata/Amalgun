using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class RandomDestinationMovement : DirectionForceMovementBase
{
    [SerializeField]
    private float _movingRecalculateTimeSeconds = 3f;
    private float _movingRecalculateTimer;
    public override void ApplyMovement(Rigidbody rb, float timePassed)
    {
        if (ShouldRecalculate(timePassed))
        {
            TargetDirection = PickDirection(rb.transform.position);
            ResetRecalculateTimer();
        }
        ApplyForce(rb);
    }

    private bool ShouldRecalculate(float timePassed) => (_movingRecalculateTimer -= timePassed) <= 0;
    private void ResetRecalculateTimer() => _movingRecalculateTimer = _movingRecalculateTimeSeconds;

    private Vector3 PickDirection(Vector3 startingPosition)
    {
        // Pick a random destination point to move towards
        float x = UnityEngine.Random.Range(-15, 15);
        float z = UnityEngine.Random.Range(-15, 15);
        Vector3 targetLocation = new(x, 0, z);

        return (targetLocation - startingPosition).normalized;
    }
}
