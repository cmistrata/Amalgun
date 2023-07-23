using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PartState
{
    Attached,
    Attachable,
    Enemy
}

public class PartStateContainer : MonoBehaviour
{
    public PartState state = PartState.Attachable;
}
