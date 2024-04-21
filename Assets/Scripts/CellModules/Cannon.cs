using UnityEngine;

public class Cannon : CellModule
{
    [Header("State")]
    public bool AutoFiring = true;
    private float _aimingAngle = 0;
    private Vector3 _aimingDirection = Vector3.forward;

    [Header("Firing Parameters")]
    public GameObject ProjectilePrefab;
    public float AutoFireIntervalSeconds = 2.5f;
    private float _autoFireTimer = 0f;
    public float FiringInaccuracyAngles = 0;
    public float InitialProjectileOffset = 0;
    public Transform InitialFiringPosition;
    public float PlayerProjectileSpeed = 10;
    public float EnemyProjectileSpeed = 4;
    public float NumProjectiles = 1;
    public float FiringSpreadAngles = 0;
    public enum TargetingStrategy
    {
        StaticDirection,
        TargetPlayer,
        TargetMouseCursor
    }
    public TargetingStrategy PlayerTargetingStrategy = TargetingStrategy.TargetMouseCursor;
    public TargetingStrategy EnemyTargetingStrategy = TargetingStrategy.TargetPlayer;
    private TargetingStrategy _currentTargetingStrategy = TargetingStrategy.StaticDirection;

    [Header("Sprites")]
    public GameObject CannonBase;

    // Start is called before the first frame update
    void Start()
    {
        HandleTeamChange(_team);
    }

    // Update is called once per frame
    void Update()
    {
        bool firingAllowedInGameState = GameManager.Instance == null || GameManager.Instance.State == GameState.Fighting;
        if (AutoFiring
            && firingAllowedInGameState
            && _team != Team.Neutral)
        {
            _autoFireTimer -= Time.deltaTime;
            if (_autoFireTimer < 0)
            {
                FireProjectiles();
                _autoFireTimer += Random.Range(.9f, 1.1f) * AutoFireIntervalSeconds;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.Paused)
        {
            _aimingDirection = GetAimingDirection();
            _aimingAngle = Mathf.Atan2(_aimingDirection.x, _aimingDirection.z) * Mathf.Rad2Deg;
            CannonBase.transform.rotation = Quaternion.AngleAxis(_aimingAngle, Vector3.up);
        }
    }

    public void FireProjectiles()
    {
        if (NumProjectiles == 1)
        {
            FireProjectile();
            return;
        }

        float currentFiringAngleOffset = -FiringSpreadAngles * .5f;
        for (int i = 0; i < NumProjectiles; i++)
        {
            FireProjectile(currentFiringAngleOffset);
            // Choose the next angle by moving a fraction of the total firing spread, e.g.:
            //   For 2 projectiles, would move the entire spread.
            //   For 3 projectiles, would move half of the spread.
            currentFiringAngleOffset += FiringSpreadAngles / (NumProjectiles - 1);
        }
    }


    void FireProjectile(float aimingAngleOffset = 0)
    {

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

        //TODO: replace with some kind of event and an audio manager
        gameObject.GetComponent<AudioSource>().Play();
    }

    Vector3 GetAimingDirection()
    {
        switch (_currentTargetingStrategy)
        {
            case TargetingStrategy.TargetPlayer:
                return Player.Instance != null ? Player.Instance.transform.position - transform.position : _aimingDirection;
            case TargetingStrategy.TargetMouseCursor:
                if (Utils.MouseRaycast(out var raycastHit))
                {
                    Vector3 mousePosition = new Vector3(raycastHit.point.x, 0, raycastHit.point.z);
                    return mousePosition - transform.position;
                }
                else
                {
                    return _aimingDirection;
                }
            case TargetingStrategy.StaticDirection:
            default:
                return _aimingDirection;
        }
    }

    protected override void HandleTeamChange(Team newTeam)
    {
        if (_team == Team.Player || _team == Team.Neutral)
        {
            //CannonSpriteRenderer.sprite = CannonSpritePlayer;
            _currentTargetingStrategy = _team == Team.Player ? PlayerTargetingStrategy : TargetingStrategy.StaticDirection;
        }
        else
        {
            //CannonSpriteRenderer.sprite = CannonSpriteEnemy;
            _currentTargetingStrategy = EnemyTargetingStrategy;
        }
    }
}
