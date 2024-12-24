using UnityEngine;

public class Cannon : CellModule {
    private Animator _animator;
    private Rigidbody _rb;
    public AudioSource FireAudioSource;


    [Header("State")]
    public bool AutoFiring = true;
    private float _aimingAngle = 0;
    private Vector3 _aimingDirection = Vector3.forward;

    [Header("Firing Parameters")]
    public GameObject ProjectilePrefab;
    public float AutoFireIntervalSeconds = 2.5f;
    private float _autoFireTimer;
    public float FiringInaccuracyAngles = 0;
    public float InitialProjectileOffset = 0;
    public Transform AimFrom;
    public Transform InitialFiringPosition;
    public float PlayerProjectileSpeed = 10;
    public float EnemyProjectileSpeed = 4;
    public float NumProjectiles = 1;
    public float FiringSpreadAngles = 0;
    public float FireAnimationOffsetTime = 0;
    public float FiringRecoilForce = 0f;
    private bool _triggeredAnimationYet = false;
    public enum TargetingStrategy {
        StaticDirection,
        TargetPlayer,
        PlayerDirected
    }
    public TargetingStrategy PlayerTargetingStrategy = TargetingStrategy.PlayerDirected;
    public TargetingStrategy EnemyTargetingStrategy = TargetingStrategy.TargetPlayer;
    private TargetingStrategy _currentTargetingStrategy = TargetingStrategy.StaticDirection;

    public GameObject CannonBase;
    public Animator CannonAnimator;

    protected override void ExtraAwake() {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        FireAudioSource = GetComponent<AudioSource>();
        if (!AimFrom) {
            AimFrom = transform;
        }
        HandleStateChange(_state);
        _autoFireTimer = .3f * AutoFireIntervalSeconds;
    }

    // Update is called once per frame
    void Update() {
        if (MenuManager.Instance.Paused) return;
        bool firingAllowedInGameState = GameManager.Instance == null || GameManager.Instance.State == GameState.Fighting;
        if (AutoFiring
            && firingAllowedInGameState
            && _state != CellState.Neutral) {
            _autoFireTimer -= Time.deltaTime;
            if (!_triggeredAnimationYet && _autoFireTimer <= FireAnimationOffsetTime) {
                if (_animator != null) {
                    _animator.SetTrigger("Fire");
                }
                if (CannonAnimator != null) {
                    CannonAnimator.SetTrigger("Fire");
                }
                _triggeredAnimationYet = true;
            }
            if (_autoFireTimer < 0) {
                FireProjectiles();
                _autoFireTimer += Random.Range(.9f, 1.1f) * AutoFireIntervalSeconds;
                _triggeredAnimationYet = false;
            }
        }
    }

    private void FixedUpdate() {
        _aimingDirection = GetAimingDirection();
        _aimingAngle = Mathf.Atan2(_aimingDirection.x, _aimingDirection.z) * Mathf.Rad2Deg;
        CannonBase.transform.rotation = Quaternion.AngleAxis(_aimingAngle, Vector3.up);
    }

    public void FireProjectiles() {
        if (NumProjectiles == 1) {
            FireProjectile();
            return;
        }

        float currentFiringAngleOffset = -FiringSpreadAngles * .5f;
        for (int i = 0; i < NumProjectiles; i++) {
            FireProjectile(currentFiringAngleOffset);
            // Choose the next angle by moving a fraction of the total firing spread, e.g.:
            //   For 2 projectiles, would move the entire spread.
            //   For 3 projectiles, would move half of the spread.
            currentFiringAngleOffset += FiringSpreadAngles / (NumProjectiles - 1);
        }
    }


    void FireProjectile(float aimingAngleOffset = 0) {
        float inaccuracyOffset = Random.Range(-FiringInaccuracyAngles, FiringInaccuracyAngles);
        float firingAngleAfterOffset = _aimingAngle + aimingAngleOffset + inaccuracyOffset;

        float projectileSpeed = _state == CellState.Friendly || _state == CellState.Melded ? PlayerProjectileSpeed : EnemyProjectileSpeed;
        Vector3 firingPosition = InitialFiringPosition.position + (InitialProjectileOffset * _aimingDirection.normalized);

        //TODO: replace with a object pool
        GameObject projectile = Instantiate(ProjectilePrefab, Containers.Bullets);
        Bullet bullet = projectile.GetComponent<Bullet>();
        bullet.SetFiringCellState(_state);
        bullet.StartStraightMotion(firingPosition, firingAngleAfterOffset, projectileSpeed);
        if (_state == CellState.Enemy && FiringRecoilForce > 0 && _rb != null) {
            var forceDirection = Quaternion.AngleAxis(firingAngleAfterOffset, Vector3.up) * -Vector3.forward;
            _rb.AddForce(forceDirection * FiringRecoilForce, ForceMode.Impulse);
        }

        //TODO: replace with some kind of event and an audio manager
        FireAudioSource.Play();
    }

    Vector3 GetAimingDirection() {
        return _currentTargetingStrategy switch {
            TargetingStrategy.TargetPlayer => (GameManager.Instance.Player != null ? GameManager.Instance.Player.transform.position - AimFrom.position : _aimingDirection).UpdateCoords(y: 0),
            TargetingStrategy.PlayerDirected => GetPlayerAimingDirection(),
            _ => _aimingDirection,
        };
    }

    Vector3 GetPlayerAimingDirection() {
        bool mouseRecentlyMoved = InputManager.MouseMovement.ReadValue<Vector2>().sqrMagnitude != 0;
        Vector2 lookInput = InputManager.Look.ReadValue<Vector2>();
        if (mouseRecentlyMoved || lookInput.sqrMagnitude == 0) {
            return (Utils.GetMousePosition() - AimFrom.position).UpdateCoords(y: 0);
        }
        else if (lookInput.sqrMagnitude != 0) {
            return new Vector3(lookInput.x, 0, lookInput.y);
        }
        else {
            return _aimingDirection;
        }
    }

    protected override void HandleStateChange(CellState newState) {
        _autoFireTimer = AutoFireIntervalSeconds * Random.Range(.3f, .8f);
        if (_state == CellState.Friendly || _state == CellState.Attaching || _state == CellState.Melded) {
            _currentTargetingStrategy = PlayerTargetingStrategy;
        }
        else if (_state == CellState.Neutral) {
            _currentTargetingStrategy = TargetingStrategy.StaticDirection;
        }
        else {
            _currentTargetingStrategy = EnemyTargetingStrategy;
        }
    }
}
