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
    private TeamTracker _teamTracker;

    [Header("Health info")]
    public float MaxHealth = 1;
    public float currentHealth = 1;
    public Team Team = Team.Enemy;

    [Range(0, 1)]
    [Header("Conversion RNG")]
    public float ConvertChance = 0.15f;
    public float destroyOnDeath = 0;

    public void Awake() {
        _teamTracker = GetComponent<TeamTracker>();
    }

    virtual public void Start() {
        currentHealth = MaxHealth;
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
        if (_teamTracker.Team == Team.Player)  {
            Player player = transform.GetComponentInParent<Player>();
            if (player == null) {
                Debug.LogError("Part was Team.Player but wasn't under the player");
            } else {
                player.DetatchPart(this);
                PlayDeathEffect();
            }
            
            if (this == Player.Instance.CenterPart) {
                Player.Instance.DestroyAllParts();
                Player.Instance.gameObject.SetActive(false);
            } else {
                Destroy(gameObject);
            }
        } else if (_teamTracker.Team == Team.Enemy) {
            // An enemy has died, decrease the count
            WavesManager.Instance.EnemyCount--;
            // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
            bool convertPart = UnityEngine.Random.Range(0f, 1f) < ConvertChance || WavesManager.Instance.EnemyCount == 0;
            if (convertPart) {
                ConvertEnemyPart();
                AudioManager.Instance.PlayUISound(1.4f);
            } else {
                PlayDeathEffect();
                Destroy(gameObject);
            }
            // If there are no enemies left, spawn the next wave
            if (WavesManager.Instance.EnemyCount == 0) {
                // Heal player to full
                var centerPart = Player.Instance.CenterPart;
                if (centerPart.currentHealth <= centerPart.MaxHealth - 1)
                {
                    centerPart.currentHealth += 1;
                }
                WavesManager.Instance.StartNextWave();
            }
        } else {
            Debug.LogError("Die() was called on unattached part.");
        }
    }

    public void PlayDeathEffect()
    {
        if (_teamTracker.Team == Team.Enemy)
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
        if (_teamTracker.Team != Team.Neutral) return;
        bool otherIsThePlayer = other.gameObject.GetComponent<Player>() != null;
        if (otherIsThePlayer)
        {
            Player.Instance.AddPart(this);
            return;
        }
        TeamTracker otherPartTeamTracker = other.gameObject.GetComponent<TeamTracker>();
        if (otherPartTeamTracker == null || otherPartTeamTracker.Team != Team.Neutral) return;

        Player.Instance.AddPart(this);
    }

    public virtual void ConvertEnemyPart()
    {
        if (_teamTracker.Team != Team.Enemy)
        {
            Debug.LogWarning("Tried to convert a part that was not an enemy");
        }
        gameObject.layer = LayerMask.NameToLayer("PlayerPart");
        _teamTracker.ChangeTeam(Team.Neutral);
        
        currentHealth = MaxHealth;
    }

}
