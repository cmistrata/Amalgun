using UnityEngine;

public enum CellType {
    None = 0,
    PlayerNucleus = 1,
    Basic = 2,
    Rocket = 3
}

public class CellProperties : MonoBehaviour {
    public CellType Type;
}
