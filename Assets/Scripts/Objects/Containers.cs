using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Containers : MonoBehaviour {
    public static Containers Instance;

    [SerializeField]
    private Transform _Bullets;
    public static Transform Bullets {
        get => Instance != null ? Instance._Bullets : null;
    }

    [SerializeField]
    private Transform _Cells;
    public static Transform Cells {
        get => Instance != null ? Instance._Cells : null;
    }

    public Transform _Effects;
    public static Transform Effects {
        get => Instance != null ? Instance._Effects : null;
    }

    void Awake() {
        Instance = this;
    }
}
