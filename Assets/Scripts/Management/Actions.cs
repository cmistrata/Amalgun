using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Actions : MonoBehaviour {
    public static Actions Instance;
    public static InputAction TurnClockwise;
    public static InputAction Cancel;

    void Awake() {
        Instance = this;
        TurnClockwise = InputSystem.actions.FindAction("TurnClockwise");
        Cancel = InputSystem.actions.FindAction("Cancel");
    }
}
