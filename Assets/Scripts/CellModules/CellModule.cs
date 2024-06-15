using UnityEngine;

[RequireComponent(typeof(Cell))]
public abstract class CellModule : MonoBehaviour {
    protected Cell _teamTracker;
    protected CellState _team {
        get {
            return _teamTracker.State;
        }
    }

    public void Awake() {
        _teamTracker = GetComponent<Cell>();
        _teamTracker.ChangeTeamEvent += HandleTeamChange;
        ExtraAwake();
    }

    public void OnEnable() {
        ExtraAwake();
    }

    virtual protected void ExtraAwake() {
        return;
    }

    virtual protected void HandleTeamChange(CellState newTeam) {
        return;
    }
}
