using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team {
    Player,
    Enemy,
    Neutral
}

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class Part : MonoBehaviour
{
    #region Rendering
    [Header("Rendering")]
    private static Sprite[] bases;
    private Sprite EnemyBase;
    private Sprite PlayerBase;
    private SpriteRenderer BaseRenderer;
    #endregion

    [Header("Health info")]
    public float MaxHealth = 1;
    public float currentHealth = 1;
    public Team Team = Team.Enemy;

    [Range(0, 1)]
    [Header("Conversion RNG")]
    public float ConvertChance = 0.15f;
    public float destroyOnDeath = 0;

    // Declare the delegate (if using non-generic pattern).
    public event Action<Team> ChangeTeamEvent;

    private Cannon _cannon;

    public void Awake() {
        _cannon = GetComponent<Cannon>();
        BaseRenderer = GetComponent<SpriteRenderer>(); ;
    }

    virtual public void Start() {
        (EnemyBase, PlayerBase) = PrefabsManager.Instance.GetRandomEnemyAndPlayerBase();

        UpdateSprites();
        currentHealth = MaxHealth;
    }

    public void ChangeTeam(Team newTeam) {
        Team = newTeam;

        UpdateSprites();
        if (GetComponent<EnemyController>() != null) GetComponent<EnemyController>().enabled = Team == Team.Enemy;
        if (Team == Team.Neutral && GetComponent<MovingBody>() != null) GetComponent<MovingBody>().StopMoving();
        if (_cannon != null) _cannon.ChangeTeam(Team);
        gameObject.layer = LayerMask.NameToLayer(
            Team == Team.Player ? Layers.PlayerPart
            : Team == Team.Enemy ? Layers.EnemyPart
            : Layers.NeutralPart
        );
        if (ChangeTeamEvent != null) ChangeTeamEvent(newTeam);
    }

    virtual protected void UpdateSprites()
    {
        if (Team == Team.Enemy)
        {
            BaseRenderer.sprite = EnemyBase;
        }
        else
        {
            BaseRenderer.sprite = PlayerBase;
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
            AudioManager.Instance.PlayCenterPartHitSound(1 + (4f - currentHealth) / 8f);
            CameraEffectsManager.Instance.FlashDamageFilter();
        }
    }

    /// <summary>
    /// Destroy the part.
    /// </summary>
    public void Die()
    {
        if (Team == Team.Player)
        {
            Player player = transform.GetComponentInParent<Player>();
            if (player == null)
            {
                Debug.LogError("Part was Team.Player but wasn't under the player");
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

        else if (Team == Team.Enemy)
        {
            // An enemy has died, decrease the count
            WavesManager.Instance.EnemyCount--;
            // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
            bool convertPart = UnityEngine.Random.Range(0f, 1f) < ConvertChance || WavesManager.Instance.EnemyCount == 0;
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
                if (centerPart.currentHealth <= centerPart.MaxHealth - 1)
                {
                    centerPart.currentHealth += 1;
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
        if (Team == Team.Enemy)
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
        if (Team != Team.Neutral) return;
        bool otherIsThePlayer = other.gameObject.GetComponent<Player>() != null;
        if (otherIsThePlayer)
        {
            Player.Instance.AddPart(this);
            return;
        }
        Part otherPart = other.gameObject.GetComponent<Part>();
        if (otherPart == null || otherPart.Team != Team.Neutral) return;

        Player.Instance.AddPart(this);
    }

    public virtual void ConvertEnemyPart()
    {
        if (Team != Team.Enemy)
        {
            Debug.LogWarning("Tried to convert a part that was not an enemy");
        }
        gameObject.layer = LayerMask.NameToLayer("PlayerPart");
        ChangeTeam(Team.Neutral);
        Team = Team.Neutral;
        
        currentHealth = MaxHealth;
        UpdateSprites();
    }

}
