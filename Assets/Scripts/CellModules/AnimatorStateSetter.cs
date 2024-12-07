using UnityEngine;

public class AnimatorStateSetter : CellModule {
    private Animator _animator;

    protected override void ExtraAwake() {
        _animator = GetComponent<Animator>();
        HandleStateChange(_state);
    }

    protected override void HandleStateChange(CellState newState) {
        _animator.SetBool("KnockedOut", newState == CellState.Neutral);
    }
}
