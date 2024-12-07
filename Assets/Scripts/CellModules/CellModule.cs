using UnityEngine;

[RequireComponent(typeof(Cell))]
public abstract class CellModule : MonoBehaviour {
    protected Cell _stateTracker;
    protected CellState _state {
        get {
            return _stateTracker.State;
        }
    }

    public void Awake() {
        _stateTracker = GetComponent<Cell>();
        _stateTracker.ChangeStateEvent += HandleStateChange;
        ExtraAwake();
    }

    public void OnEnable() {
        ExtraAwake();
    }

    virtual protected void ExtraAwake() {
        return;
    }

    virtual protected void HandleStateChange(CellState newState) {
        return;
    }
}
