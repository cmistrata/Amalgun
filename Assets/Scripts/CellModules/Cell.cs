using System;
using System.Collections.Generic;
using UnityEngine;

public enum CellState {
    Friendly,
    Enemy,
    Neutral,
    Absorbing,
    BeingAbsorbed,
    Attaching,
    Melded,
}

public enum CellType {
    None = 0,
    Basic = 1, Basic2 = 2, Basic3 = 3,
    Rocket = 4, Rocket2 = 5, Rocket3 = 6,
    Mine = 7, Mine2 = 8, Mine3 = 9,
    Tri = 10, Tri2 = 11, Tri3 = 12,
    Shield = 13, Shield2 = 14, Shield3 = 15
}

public class Cell : MonoBehaviour {

    public CellState State = CellState.Neutral;
    public CellType Type;
    private CellState _previousState;
    public event Action<CellState> ChangeStateEvent;
    [HideInInspector]
    public Rigidbody rb;

    // Start is called before the first frame update
    void Awake() {
        _previousState = State;
        rb = GetComponent<Rigidbody>();
    }

    void Start() {
        ChangeState(State);
    }

    private void Update() {
        // For when State gets updated through the inspector or by direct attribute access.
        // TODO: Add an editor widget to call ChangeState in the inspector, and remove
        // this logic and public access to State.
        if (State != _previousState) {
            ChangeState(State);
        }
    }

    public void ChangeState(CellState newState) {
        _previousState = newState;
        State = newState;
        gameObject.layer =
            State == CellState.Friendly || State == CellState.Absorbing || State == CellState.Melded ? Layers.PlayerCell
            : State == CellState.Enemy ? Layers.EnemyCell
            : State == CellState.Neutral || State == CellState.Attaching ? Layers.NeutralCell
            : State == CellState.Absorbing || State == CellState.BeingAbsorbed ? Layers.NoCollision
            : Layers.NoCollision;
        bool cellInCollidableState = State == CellState.Neutral || State == CellState.Enemy || State == CellState.Attaching;
        if (rb != null && !cellInCollidableState) {
            DisableRigidbody();
        }
        else if (rb == null && cellInCollidableState) {
            EnableRigidbody();
        }
        ChangeStateEvent?.Invoke(newState);
    }

    void DisableRigidbody() {
        Destroy(GetComponent<Rigidbody>());
    }

    void EnableRigidbody() {
        if (rb != null) return;
        Rigidbody newRigidbody = gameObject.AddComponent<Rigidbody>();
        newRigidbody.mass = 15;
        newRigidbody.linearDamping = 2;
        newRigidbody.angularDamping = 5;
        newRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        newRigidbody.useGravity = false;
        rb = newRigidbody;
    }
}
