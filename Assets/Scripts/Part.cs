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
    public event Action SignalEnemyDeath;

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
    }

    /// <summary>
    /// Destroy the part.
    /// </summary>
    public void Die()
    {
        if (_teamTracker.Team == Team.Enemy) {
            SignalEnemyDeath?.Invoke();
            // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
            bool convertPart = UnityEngine.Random.Range(0f, 1f) < ConvertChance;
            if (convertPart) {
                ConvertEnemyPart();
                AudioManager.Instance.PlayUISound(1.4f);
            } else {
                PlayDeathFX();
                Destroy(gameObject);
            }
        } else {
            PlayDeathFX();
            Destroy(gameObject);
        }
    }

    public void PlayDeathFX()
    {
        
        if (_teamTracker.Team == Team.Enemy)
        {
            Instantiate(PrefabsManager.Instance.EnemyDeathEffect, transform.position, Quaternion.identity);
            AudioManager.Instance.PlayEnemyDestroy();
            CameraManager.Instance.ShakeCamera(.3f, .3f);
        }
        else
        {
            Instantiate(PrefabsManager.Instance.PlayerDeathEffect, transform.position, Quaternion.identity);
            AudioManager.Instance.PlayPartDestroy();
            CameraManager.Instance.ShakeCamera(.1f, .1f);
        }
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

    public void OnCollisionEnter2D(Collision2D collision) {
        // Only handle bullet collisions.
        // Furthhermore, only handle bullet collisions for enemies. Player bullet collisions
        // will be handled in the Player object.
        if (_teamTracker.Team == Team.Enemy && collision.gameObject.layer == Layers.PlayerBullet) {
            Die();
        }
    }
}
