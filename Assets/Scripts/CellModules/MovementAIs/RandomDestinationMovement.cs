using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class RandomDestinationMovement : DirectionForceMovementBase
{
    [SerializeField]
    private float _timerPeriod = 4f;

    private FixedDelayTimer _timer;

    RandomDestinationMovement() : base()
    {
        _timer = new(_timerPeriod);
    }

    public override void ApplyMovement(Rigidbody rb, float timePassed)
    {
        if (_timer.HasTimerTripped(timePassed) || TargetDirection == Vector3.zero)
        {
            TargetDirection = PickDirection(rb.transform.position);
        }
        ApplyForce(rb);
    }

    private Vector3 PickDirection(Vector3 startingPosition)
    {
        // Pick a random destination point to move towards
        float x = UnityEngine.Random.Range(-15, 15);
        float z = UnityEngine.Random.Range(-15, 15);
        Vector3 targetLocation = new(x, 0, z);

        return (targetLocation - startingPosition).normalized;
    }
}
