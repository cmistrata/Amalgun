using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team {
    Player,
    Enemy,
    Neutral
}

public class Cannon : MonoBehaviour
{
    [Header("State")]
    public Team Team = Team.Neutral;
    
    public bool AutoFiring = true;
    private float FiringAngle = 0;
    private Vector2 _firingDirection = Vector2.up;

    [Header("Firing Parameters")]
    public GameObject ProjectilePrefab;
    public float AutoFireIntervalSeconds = 3;
    public float FireAccuracyInAngles = 0;
    public float InitialProjectileOffset = 0.5f;
    public float InitialProjectileSpeed = 10;
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


    // Start is called before the first frame update
    void Start()
    {
        if (AutoFiring && Team != Team.Neutral) {
            Invoke("Fire", AutoFireIntervalSeconds);
        }


        if (Team == Team.Player || Team == Team.Neutral) {
            CannonSpriteRenderer.sprite = CannonSpritePlayer;
            _currentTargetingStrategy = Team == Team.Player ? PlayerTargetingStrategy : TargetingStrategy.StaticDirection;
        } else {
            CannonSpriteRenderer.sprite = CannonSpriteEnemy;
            _currentTargetingStrategy = EnemyTargetingStrategy;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _firingDirection = GetFiringDirection();
        FiringAngle = Mathf.Atan2(_firingDirection.y, _firingDirection.x) * Mathf.Rad2Deg;
        CannonSpriteRenderer.transform.rotation = Quaternion.AngleAxis(FiringAngle - 90, Vector3.forward);

        if (AutoFiring && Team != Team.Neutral && !IsInvoking()) {
            Invoke("Fire", AutoFireIntervalSeconds);
        }
    }

    void Fire() {
        if (AutoFiring) {
            FireProjectile(_firingDirection);
            Invoke("Fire", AutoFireIntervalSeconds);
        }
    }

    /// <summary>
    /// Shoots a projectile from this game object towards the screen point given
    /// </summary>
    /// <param name="screenPointTarget"></param>
    /// <param name="projectilePrefab"></param>
    /// <param name="isPlayer"></param>
    protected virtual void FireProjectile(Vector2 firingDirection) {
        float spread = Random.Range(-FireAccuracyInAngles, FireAccuracyInAngles);
        float firingAngleAfterOffset = FiringAngle + spread;

        GameObject projectile = Instantiate(
            ProjectilePrefab,
            transform.position + (Vector3)(InitialProjectileOffset * _firingDirection.normalized),
            Quaternion.AngleAxis(firingAngleAfterOffset, Vector3.forward),
            GameObjectsContainer.Instance.bulletsContainer
        );
        Bullet bullet = projectile.GetComponent<Bullet>();

        SpriteRenderer sRender = bullet.gameObject.GetComponent<SpriteRenderer>();
        sRender.sprite = Team == Team.Enemy ? BulletSpriteEnemy : BulletSpritePlayer;

        bullet.Damage = 100;
        bullet.TimeOutSeconds = 5;
        bullet.isPlayerBullet = Team == Team.Player;
        projectile.GetComponent<Rigidbody2D>().velocity = projectile.transform.right * InitialProjectileSpeed;
        gameObject.GetComponent<AudioSource>().Play();
    }

    Vector2 GetFiringDirection() {
        switch (_currentTargetingStrategy) {
            case TargetingStrategy.TargetPlayer:
                return Player.Instance != null ? Player.Instance.transform.position - transform.position : _firingDirection;
            case TargetingStrategy.TargetMouseCursor:
                Vector3 cameraPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                return cameraPosition - transform.position;
            case TargetingStrategy.StaticDirection: default:
                return _firingDirection;
        }
    }

    public void ChangeTeam(Team newTeam) {
        Team = newTeam;
        if (Team == Team.Player || Team == Team.Neutral) {
            CannonSpriteRenderer.sprite = CannonSpritePlayer;
            _currentTargetingStrategy = Team == Team.Player ? PlayerTargetingStrategy : TargetingStrategy.StaticDirection;
        } else {
            CannonSpriteRenderer.sprite = CannonSpriteEnemy;
            _currentTargetingStrategy = EnemyTargetingStrategy;
        }
    }
}
