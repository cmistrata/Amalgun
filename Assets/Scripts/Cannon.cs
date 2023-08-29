using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;

//[RequireComponent((Part))]
public class Cannon : MonoBehaviour
{
    private TeamTracker _teamTracker;

    [Header("State")]
    public bool AutoFiring = true;
    public float AimingAngle = 0;
    private Vector2 _aimingDirection = Vector2.up;

    [Header("Firing Parameters")]
    public GameObject ProjectilePrefab;
    public float AutoFireIntervalSeconds = 2.5f;
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
    public Sprite CannonSpriteEnemy;
    public Sprite CannonSpritePlayer;
    public SpriteRenderer CannonSpriteRenderer;
    public Sprite BulletSpriteEnemy;
    public Sprite BulletSpritePlayer;

    private void Awake() {
        _teamTracker = GetComponent<TeamTracker>();
        _teamTracker.ChangeTeamEvent += this.HandleChangeTeam;
    }


    // Start is called before the first frame update
    void Start()
    {
        if (AutoFiring && _teamTracker.Team != Team.Neutral) {
            Invoke("Fire", AutoFireIntervalSeconds);
        }


        HandleChangeTeam(_teamTracker.Team);
    }

    // Update is called once per frame
    void Update()
    {
        _aimingDirection = GetAimingDirection();
        AimingAngle = Mathf.Atan2(_aimingDirection.y, _aimingDirection.x) * Mathf.Rad2Deg;
        CannonSpriteRenderer.transform.rotation = Quaternion.AngleAxis(AimingAngle - 90, Vector3.forward);

        if (AutoFiring && _teamTracker.Team != Team.Neutral && !IsInvoking()) {
            Invoke("Fire", AutoFireIntervalSeconds);
        }
    }

    void Fire() {
        if (AutoFiring && _teamTracker.Team != Team.Neutral) {
            FireProjectiles();
            Invoke("Fire", AutoFireIntervalSeconds);
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
        float firingAngleAfterOffset = AimingAngle + aimingAngleOffset + inaccuracyOffset;

        GameObject projectile = Instantiate(
            ProjectilePrefab,
            transform.position + (Vector3)(InitialProjectileOffset * _aimingDirection.normalized),
            Quaternion.AngleAxis(firingAngleAfterOffset, Vector3.forward),
            BulletsContainer.Instance.transform
        );
        Bullet bullet = projectile.GetComponent<Bullet>();

        SpriteRenderer sRender = bullet.gameObject.GetComponent<SpriteRenderer>();
        sRender.sprite = _teamTracker.Team == Team.Enemy ? BulletSpriteEnemy : BulletSpritePlayer;

        bullet.TimeOutSeconds = 5;
        projectile.GetComponent<Rigidbody2D>().velocity = projectile.transform.right * InitialProjectileSpeed;
        projectile.layer = _teamTracker.Team == Team.Enemy ? Layers.EnemyBullet : Layers.PlayerBullet;
        gameObject.GetComponent<AudioSource>().Play();
    }

    Vector2 GetAimingDirection() {
        switch (_currentTargetingStrategy) {
            case TargetingStrategy.TargetPlayer:
                return Player.Instance != null ? Player.Instance.transform.position - transform.position : _aimingDirection;
            case TargetingStrategy.TargetMouseCursor:
                Vector3 cameraPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                return cameraPosition - transform.position;
            case TargetingStrategy.StaticDirection: default:
                return _aimingDirection;
        }
    }

    public void HandleChangeTeam(Team newTeam) {
        if (_teamTracker.Team == Team.Player || _teamTracker.Team == Team.Neutral) {
            CannonSpriteRenderer.sprite = CannonSpritePlayer;
            _currentTargetingStrategy = _teamTracker.Team == Team.Player ? PlayerTargetingStrategy : TargetingStrategy.StaticDirection;
        } else {
            CannonSpriteRenderer.sprite = CannonSpriteEnemy;
            _currentTargetingStrategy = EnemyTargetingStrategy;
        }
    }
}
