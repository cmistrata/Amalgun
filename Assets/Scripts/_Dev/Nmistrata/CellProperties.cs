using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CellType
{
    None = 0,
    Basic = 1
}

public class CellProperties : MonoBehaviour
{
    [SerializeField]
    public CellType Type;
}
