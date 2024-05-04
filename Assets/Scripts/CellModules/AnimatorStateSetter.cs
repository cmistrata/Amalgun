using UnityEngine;

public class AnimatorStateSetter : CellModule {
    private Animator _animator;

    protected override void ExtraAwake() {
        _animator = GetComponent<Animator>();
        HandleTeamChange(_team);
    }

    protected override void HandleTeamChange(Team newTeam) {
        _animator.SetBool("KnockedOut", newTeam == Team.Neutral);
    }
}
