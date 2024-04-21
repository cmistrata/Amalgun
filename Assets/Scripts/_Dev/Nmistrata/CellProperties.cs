using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CellType
{
    None = 0,
    PlayerNucleus = 1,
    Basic = 2
}

[RequireComponent(typeof(PlayerShipPiece))]
[RequireComponent(typeof(MovementBase))]
[RequireComponent(typeof(CellHealthManager))]
[RequireComponent(typeof(TeamTracker))]
public class CellProperties : MonoBehaviour
{
    [SerializeField]
    public CellType Type;
}
