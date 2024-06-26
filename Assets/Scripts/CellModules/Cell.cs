using System;
using UnityEngine;

public enum CellState {
    Player,
    Enemy,
    Neutral,
    Absorbing,
    BeingAbsorbed
}

public enum CellType {
    None = 0,
    Basic = 1,
    Basic2 = 3,
    Rocket = 2
}

public class Cell : MonoBehaviour {

    public CellState State = CellState.Neutral;
    public CellType Type;
    private CellState _previousTeam;
    public event Action<CellState> ChangeTeamEvent;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Awake() {
        _previousTeam = State;
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        // For when Team gets updated through the inspector or by direct attribute access.
        // TODO: Add an editor widget to call ChangeTeam in the inspector, and remove
        // this logic and public access to Team.
        if (State != _previousTeam) {
            ChangeState(State);
        }
    }

    public void ChangeState(CellState newTeam) {
        _previousTeam = newTeam;
        State = newTeam;
        gameObject.layer =
            State == CellState.Player || State == CellState.Absorbing ? Layers.PlayerCell
            : State == CellState.Enemy ? Layers.EnemyCell
            : State == CellState.Neutral ? Layers.NeutralCell
            : Layers.NoCollision;
        bool cellInCollidableState = State == CellState.Neutral || State == CellState.Enemy || State == CellState.Absorbing;
        if (rb != null && !cellInCollidableState) {
            DisableRigidbody();
        }
        else if (rb == null && cellInCollidableState) {
            EnableRigidbody();
        }
        ChangeTeamEvent?.Invoke(newTeam);
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
