using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager Instance;
    public static InputAction TurnClockwise;
    public static InputAction Cancel;
    public static InputAction Dash;
    public static InputAction Move;
    public static InputAction MoveAnalogue;
    public static InputAction Submit;
    public static InputAction Menu;
    public static InputAction MouseMovement;
    public static InputAction Look;

    void Awake() {
        Instance = this;
        TurnClockwise = InputSystem.actions.FindAction("TurnClockwise");
        Cancel = InputSystem.actions.FindAction("Cancel");
        Dash = InputSystem.actions.FindAction("Dash");
        Move = InputSystem.actions.FindAction("Move");
        MoveAnalogue = InputSystem.actions.FindAction("MoveAnalogue");
        Submit = InputSystem.actions.FindAction("Submit");
        Menu = InputSystem.actions.FindAction("Menu");
        MouseMovement = InputSystem.actions.FindAction("MouseMovement");
        Look = InputSystem.actions.FindAction("Look");
    }
}
