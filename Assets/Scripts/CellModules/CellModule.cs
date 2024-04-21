using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(TeamTracker))]
public abstract class CellModule : MonoBehaviour {
    private TeamTracker _teamTracker;
    protected Team _team {
        get {
            return _teamTracker.Team;
        }
    }

    public void Awake() {
        _teamTracker = GetComponent<TeamTracker>();
        _teamTracker.ChangeTeamEvent += HandleTeamChange;
        ExtraAwake();
    }

    public void OnEnable() {
        ExtraAwake();
    }

    virtual protected void ExtraAwake() {
        return;
    }

    virtual protected void HandleTeamChange(Team newTeam) {
        return;
    }
}
