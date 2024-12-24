using UnityEngine;

[RequireComponent(typeof(Mover))]
public class MovementAI : CellModule {
    public float MovingRecalculateIntervalSeconds = 3f;
    private float _movingRecalculateTimer;
    private Mover _mover;
    protected override void ExtraAwake() {
        _mover = GetComponent<Mover>();
    }

    void Start() {
        if (_state != CellState.Enemy) {
            enabled = false;
            return;
        }
        _mover.TargetDirection = DetermineTargetDirection();
        _movingRecalculateTimer = MovingRecalculateIntervalSeconds;
    }

    void Update() {
        if (MenuManager.Instance.Paused) return;
        // Update moving timer
        _movingRecalculateTimer -= Time.deltaTime;
        if (_movingRecalculateTimer <= 0) {
            _mover.TargetDirection = DetermineTargetDirection();
            _movingRecalculateTimer = MovingRecalculateIntervalSeconds;
        }
    }

    public Vector3 DetermineTargetDirection() {
        // Move randomly
        float x = UnityEngine.Random.Range(-15, 15);
        float z = UnityEngine.Random.Range(-15, 15);
        Vector3 targetLocation = new(x, 0, z);
        return (targetLocation - transform.position).normalized;
    }

    protected override void HandleStateChange(CellState newState) {
        if (newState != CellState.Enemy) {
            _mover.StopMoving();
        }
        enabled = newState == CellState.Enemy;
    }
}
