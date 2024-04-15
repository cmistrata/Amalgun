using UnityEngine;

public interface IMovement
{
    void ApplyMovement(Rigidbody rb, float timePassed);
}
