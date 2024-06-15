using UnityEngine;

public class AnimatorStateSetter : CellModule {
    private Animator _animator;

    protected override void ExtraAwake() {
        _animator = GetComponent<Animator>();
        HandleTeamChange(_team);
    }

    protected override void HandleTeamChange(CellState newTeam) {
        _animator.SetBool("KnockedOut", newTeam == CellState.Neutral);
    }
}
