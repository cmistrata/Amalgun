using UnityEngine;

public enum CellType {
    None = 0,
    Basic = 1,
    Basic2 = 3,
    Rocket = 2
}

public class CellProperties : MonoBehaviour {
    public CellType Type;
}
