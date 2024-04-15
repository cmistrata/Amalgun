using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMovement : ScriptableObject, IMovement
{
    public abstract void ApplyMovement(Rigidbody rb, float timePassed);
}
