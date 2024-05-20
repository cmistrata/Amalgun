using UnityEngine;

public class Cannon : CellModule {
    private Animator _animator;
    private Rigidbody _rb;
    private AudioSource _audioSource;


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
        TargetMouseCursor
    }
    public TargetingStrategy PlayerTargetingStrategy = TargetingStrategy.TargetMouseCursor;
    public TargetingStrategy EnemyTargetingStrategy = TargetingStrategy.TargetPlayer;
    private TargetingStrategy _currentTargetingStrategy = TargetingStrategy.StaticDirection;

    public GameObject CannonBase;
    public Animator CannonAnimator;

    protected override void ExtraAwake() {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _autoFireTimer = AutoFireIntervalSeconds;
        if (!AimFrom) {
            AimFrom = transform;
        }
        HandleTeamChange(_team);
    }

    // Update is called once per frame
    void Update() {
        bool firingAllowedInGameState = GameManager.Instance == null || GameManager.Instance.State == GameState.Fighting;
        if (AutoFiring
            && firingAllowedInGameState
            && _team != Team.Neutral) {
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
        if (!GameManager.Instance.Paused) {
            _aimingDirection = GetAimingDirection();
            _aimingAngle = Mathf.Atan2(_aimingDirection.x, _aimingDirection.z) * Mathf.Rad2Deg;
            CannonBase.transform.rotation = Quaternion.AngleAxis(_aimingAngle, Vector3.up);
        }
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

        float projectileSpeed = _team == Team.Player ? PlayerProjectileSpeed : EnemyProjectileSpeed;
        Vector3 firingPosition = InitialFiringPosition.position + (InitialProjectileOffset * _aimingDirection.normalized);

        //TODO: replace with a object pool
        GameObject projectile = Instantiate(ProjectilePrefab, BulletsContainer.Instance?.transform);
        Bullet bullet = projectile.GetComponent<Bullet>();
        bullet.ChangeTeam(_team);
        bullet.StartStraightMotion(firingPosition, firingAngleAfterOffset, projectileSpeed);
        bullet.SetTimeout(5);
        if (_team == Team.Enemy && FiringRecoilForce > 0 && _rb != null) {
            var forceDirection = Quaternion.AngleAxis(firingAngleAfterOffset, Vector3.up) * -Vector3.forward;
            _rb.AddForce(forceDirection * FiringRecoilForce, ForceMode.Impulse);
        }

        //TODO: replace with some kind of event and an audio manager
        _audioSource.Play();
    }

    Vector3 GetAimingDirection() {
        return _currentTargetingStrategy switch {
            TargetingStrategy.TargetPlayer => (GameManager.Instance.CurrentPlayer != null ? GameManager.Instance.CurrentPlayer.transform.position - AimFrom.position : _aimingDirection).UpdateCoords(y: 0),
            TargetingStrategy.TargetMouseCursor => (Utils.GetMousePosition() - AimFrom.position).UpdateCoords(y: 0),
            _ => _aimingDirection,
        };
    }

    protected override void HandleTeamChange(Team newTeam) {
        _autoFireTimer = AutoFireIntervalSeconds;
        if (_team == Team.Player || _team == Team.Neutral) {
            _currentTargetingStrategy = _team == Team.Player ? PlayerTargetingStrategy : TargetingStrategy.StaticDirection;
        }
        else {
            _currentTargetingStrategy = EnemyTargetingStrategy;
        }
    }
}
