using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class Part : MonoBehaviour
{
    #region Rendering
    [Header("Rendering")]
    private static Sprite[] bases;
    private Sprite EnemyBase;
    private Sprite PlayerBase;
    public Sprite EnemyTurret;
    public Sprite PlayerTurret;
    public Sprite EnemyBulletSprite;
    public Sprite PlayerBulletSprite;
    private SpriteRenderer BaseRenderer;
    private SpriteRenderer TurretRenderer;
    public bool Swivels = true;
    #endregion

    [Header("Health info")]
    public float MaxHealth = 100;
    public float currentHealth;
    public int BulletDamage = 100;
    public float BulletTtlSeconds = 5;
    public int ContactDamage = 25;
    public PartState state;

    [Range(0, 1)]
    [Header("Conversion RNG")]
    public float ConvertChance = 0.15f;

    #region Shooting parameters
    [Header("Shooting Parameters")]
    public GameObject ProjectilePrefab;
    public float ProjectileVelocity = 10;
    public float InitialProjectileOffset = 0;
    public float ShotsPerSecond = 2;
    public float ShotSpreadDegrees = 0f;
    protected float shotInterval;
    protected float shotTimer = 0;
    public float destroyOnDeath = 0;
    #endregion

    public EnemyController EnemyLogic = null;

    private Cannon _cannon;

    public void Awake() {
        _cannon = GetComponent<Cannon>();
    }

    virtual public void Start() {
        (EnemyBase, PlayerBase) = PrefabsManager.Instance.GetRandomEnemyAndPlayerBase();
        if (state != PartState.Enemy) {
            Destroy(GetComponent<EnemyController>());
        }
        else {
            EnemyLogic = GetComponent<EnemyController>();
            shotTimer = shotInterval + 1;
        }

        BaseRenderer = GetComponent<SpriteRenderer>();
        TurretRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
        UpdateSprites();
        shotInterval = 1 / ShotsPerSecond;
        currentHealth = MaxHealth;
    }

    virtual public void Update()
    {
        switch (state)
        {
            case PartState.Enemy:
                EnemyUpdate();
                break;
            case PartState.Attachable:
                AttachableUpdate();
                break;
            case PartState.Attached:
                AttachedUpdate();
                break;
        }
    }

    private void EnemyUpdate()
    {
    }

    private void AttachableUpdate()
    {
        Vector3 playerPos = Player.Instance ? Player.Instance.gameObject.transform.position : new Vector3(1000000, 1000000, 1000000);
        Vector3 dir = (playerPos - transform.position).normalized;
        GetComponent<Rigidbody2D>().velocity = dir * .5f;
    }

    private void AttachedUpdate()
    {
    }

    private void ResetShotTimer()
    {
        shotTimer = shotInterval * Random.Range(.95f, 1.05f);
    }

    virtual protected void UpdateSprites()
    {
        if (state == PartState.Enemy)
        {
            BaseRenderer.sprite = EnemyBase;
            TurretRenderer.sprite = EnemyTurret;
        }
        else
        {
            BaseRenderer.sprite = PlayerBase;
            TurretRenderer.sprite = PlayerTurret;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else if (Player.Instance.CenterPart == this)
        {
            AudioManager.Instance.PlayCenterPartHitSound(1 + (400f - currentHealth) / 800f);
            CameraEffectsManager.Instance.FlashDamageFilter();
        }
    }

    /// <summary>
    /// Destroy the part.
    /// </summary>
    public void Die()
    {
        if (state == PartState.Attached)
        {
            Player player = transform.GetComponentInParent<Player>();
            if (player == null)
            {
                Debug.LogError("Part had state Attached but wasn't under the player");
            }
            else
            {
                player.DetatchPart(this);
                PlayDeathEffect();
            }
            if (this == Player.Instance.CenterPart)
            {
                Player.Instance.DestroyAllParts();
                Player.Instance.gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        else if (state == PartState.Enemy)
        {
            // An enemy has died, decrease the count
            WavesManager.Instance.EnemyCount--;
            // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
            bool convertPart = Random.Range(0f, 1f) < ConvertChance || WavesManager.Instance.EnemyCount == 0;
            if (convertPart)
            {
                ConvertEnemyPart();
                AudioManager.Instance.PlayUISound(1.4f);
            }
            else
            {
                PlayDeathEffect();
                Destroy(gameObject);
            }
            // If there are no enemies left, spawn the next wave
            if (WavesManager.Instance.EnemyCount == 0)
            {
                // Heal player to full
                var centerPart = Player.Instance.CenterPart;
                if (centerPart.currentHealth <= centerPart.MaxHealth - 100)
                {
                    centerPart.currentHealth += 100;
                }
                WavesManager.Instance.StartNextWave();
            }
        }
        else
        {
            Debug.LogError("Die() was called on unattached part.");
        }
    }

    public void PlayDeathEffect()
    {
        if (state == PartState.Enemy)
        {
            Instantiate(PrefabsManager.Instance.EnemyDeathEffect, transform.position, Quaternion.identity);
            AudioManager.Instance.PlayEnemyDestroy();
            CameraEffectsManager.Instance.ShakeCamera(.3f, .3f);
        }
        else
        {
            Instantiate(PrefabsManager.Instance.PlayerDeathEffect, transform.position, Quaternion.identity);
            AudioManager.Instance.PlayPartDestroy();
            CameraEffectsManager.Instance.ShakeCamera(.1f, .1f);
        }
        Destroy(gameObject);
    }

    public void RestoreHealth()
    {
        currentHealth = MaxHealth;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (state != PartState.Attachable) return;
        bool otherIsThePlayer = other.gameObject.GetComponent<Player>() != null;
        if (otherIsThePlayer)
        {
            Player.Instance.AddPart(this);
            return;
        }
        Part otherPart = other.gameObject.GetComponent<Part>();
        if (otherPart == null || otherPart.state != PartState.Attached) return;

        Player.Instance.AddPart(this);
    }

    public virtual void ConvertEnemyPart()
    {
        if (state != PartState.Enemy)
        {
            Debug.LogWarning("Tried to convert a part that was not an enemy");
        }
        gameObject.layer = LayerMask.NameToLayer("PlayerPart");
        state = PartState.Attachable;
        if (_cannon != null) _cannon.ChangeTeam(Team.Neutral);
        currentHealth = MaxHealth;
        UpdateSprites();
        GetComponent<Rigidbody2D>().isKinematic = false;

        // Floor speed at 10
        ProjectileVelocity = ProjectileVelocity > 0 ? Mathf.Max(ProjectileVelocity, 10) : 0;
        // Up attack speed
        ShotsPerSecond *= 1.2f;
        shotInterval = 1 / ShotsPerSecond;
        Destroy(GetComponent<EnemyController>());
    }

}
