using UnityEngine;

public abstract class CellModule : MonoBehaviour {
    private bool _isPlayer;
    protected Cell _stateTracker;
    protected CellState _state {
        get {
            return _isPlayer ? CellState.Melded : _stateTracker.State;
        }
    }

    public void Awake() {
        // Allow using cell modules on the player without having a cell class on the player itself.
        // This means we don't need to differentiate the player from other cells when doing logic in scripts.
        _isPlayer = TryGetComponent<Player>(out var _);
        if (!_isPlayer) {
            _stateTracker = GetComponent<Cell>();
            _stateTracker.ChangeStateEvent += HandleStateChange;
        }
        ExtraAwake();
    }

    virtual protected void ExtraAwake() {
        return;
    }

    virtual protected void HandleStateChange(CellState newState) {
        return;
    }

    protected bool InActiveState() {
        return _state == CellState.Friendly || _state == CellState.Enemy || _state == CellState.Melded;
    }

    protected bool InAlliedState() {
        return _state == CellState.Friendly
        || _state == CellState.Melded
        || _state == CellState.Absorbing
        || _state == CellState.BeingAbsorbed
        || _state == CellState.Attaching;
    }
}
