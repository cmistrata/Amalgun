using UnityEngine;

public class Cannon : MonoBehaviour {
    private TeamTracker _teamTracker;

    [Header("State")]
    public bool AutoFiring = true;
    private float _aimingAngle = 0;
    private Vector3 _aimingDirection = Vector3.forward;

    [Header("Firing Parameters")]
    public GameObject ProjectilePrefab;
    public float AutoFireIntervalSeconds = 2.5f;
    private float _autoFireTimer = 0f;
    public float FiringInaccuracyAngles = 0;
    public float InitialProjectileOffset = 0.5f;
    public float InitialProjectileSpeed = 6;
    public float NumProjectiles = 1;
    public float FiringSpreadAngles = 0;
    public enum TargetingStrategy {
        StaticDirection,
        TargetPlayer,
        TargetMouseCursor
    }
    public TargetingStrategy PlayerTargetingStrategy = TargetingStrategy.TargetMouseCursor;
    public TargetingStrategy EnemyTargetingStrategy = TargetingStrategy.TargetPlayer;
    private TargetingStrategy _currentTargetingStrategy = TargetingStrategy.StaticDirection;

    [Header("Sprites")]
    public GameObject CannonBase;

    private void Awake() {
        _teamTracker = GetComponent<TeamTracker>();
        _teamTracker.ChangeTeamEvent += this.HandleChangeTeam;
    }


    // Start is called before the first frame update
    void Start() {
        HandleChangeTeam(_teamTracker.Team);
    }

    // Update is called once per frame
    void Update() {
        if (!GameManager.Instance.Paused) {
            _aimingDirection = GetAimingDirection();
            _aimingAngle = Mathf.Atan2(_aimingDirection.x, _aimingDirection.z) * Mathf.Rad2Deg;
            CannonBase.transform.rotation = Quaternion.AngleAxis(_aimingAngle, Vector3.up);
        }

        if (AutoFiring 
            //&& GameManager.Instance.State == GameState.Fighting 
            && _teamTracker.Team != Team.Neutral) {
            _autoFireTimer -= Time.deltaTime;
            if (_autoFireTimer < 0) {
                FireProjectiles();
                _autoFireTimer += AutoFireIntervalSeconds;
            }
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

        Vector3 bulletOffset = new Vector3(0, .6f, 0) + (InitialProjectileOffset * _aimingDirection.normalized);
        GameObject projectile = Instantiate(
            ProjectilePrefab,
            transform.position + bulletOffset,
            Quaternion.AngleAxis(firingAngleAfterOffset, Vector3.up)
            //BulletsContainer.Instance.transform
        );
        Bullet bullet = projectile.GetComponent<Bullet>();
        Debug.Log("Firing bullet");

        //SpriteRenderer sRender = bullet.gameObject.GetComponent<SpriteRenderer>();
        //sRender.sprite = _teamTracker.Team == Team.Enemy ? BulletSpriteEnemy : BulletSpritePlayer;

        bullet.TimeOutSeconds = 5;
        projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * InitialProjectileSpeed;
        projectile.layer = _teamTracker.Team == Team.Enemy ? Layers.EnemyBullet : Layers.PlayerBullet;
        gameObject.GetComponent<AudioSource>().Play();
    }

    Vector3 GetAimingDirection() {
        switch (_currentTargetingStrategy) {
            case TargetingStrategy.TargetPlayer:
                return Player.Instance != null ? Player.Instance.transform.position - transform.position : _aimingDirection;
            case TargetingStrategy.TargetMouseCursor:
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100)) {
                    Vector3 mousePosition = new Vector3(hit.point.x, 0, hit.point.z);
                    return mousePosition - transform.position;
                } else {
                    return _aimingDirection;
                }
            case TargetingStrategy.StaticDirection:
            default:
                return _aimingDirection;
        }
    }

    public void HandleChangeTeam(Team newTeam) {
        if (_teamTracker.Team == Team.Player || _teamTracker.Team == Team.Neutral) {
            //CannonSpriteRenderer.sprite = CannonSpritePlayer;
            _currentTargetingStrategy = _teamTracker.Team == Team.Player ? PlayerTargetingStrategy : TargetingStrategy.StaticDirection;
        } else {
            //CannonSpriteRenderer.sprite = CannonSpriteEnemy;
            _currentTargetingStrategy = EnemyTargetingStrategy;
        }
    }
}
