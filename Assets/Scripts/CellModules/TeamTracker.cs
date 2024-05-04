using System;
using UnityEngine;

public class TeamTracker : MonoBehaviour {
    public Team Team = Team.Neutral;
    private Team _previousTeam;
    public event Action<Team> ChangeTeamEvent;

    // Start is called before the first frame update
    void Awake() {
        _previousTeam = Team;
    }

    private void Update() {
        // For when Team gets updated through the inspector or by direct attribute access.
        // TODO: Add an editor widget to call ChangeTeam in the inspector, and remove
        // this logic and public access to Team.
        if (Team != _previousTeam) {
            ChangeTeam(Team);
        }
    }

    public void ChangeTeam(Team newTeam) {
        _previousTeam = newTeam;
        Team = newTeam;
        gameObject.layer =
            Team == Team.Player ? Layers.PlayerCell
            : Team == Team.Enemy ? Layers.EnemyCell
            : Layers.NeutralCell;
        ChangeTeamEvent?.Invoke(newTeam);
    }
}
