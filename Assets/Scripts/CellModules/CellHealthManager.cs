using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team {
    Player,
    Enemy,
    Neutral
}

[RequireComponent(typeof(AudioSource))]
public class CellHealthManager : MonoBehaviour
{
    private TeamTracker _teamTracker;

    [Header("Health info")]
    public float MaxHealth = 1;
    public float CurrentHealth = 1;
    public static event Action SignalEnemyDeath;

    [Range(0, 1)]
    [Header("Conversion RNG")]
    public float ConvertChance = 0.15f;
    private float CoinDropChance = 1f;
    public bool Melded = false;
    /// <summary>
    /// For use by Player to know if the cell needs to be removed from its cell graph.
    /// </summary>
    public bool BeingDestroyed = false;

    public void Awake() {
        _teamTracker = GetComponent<TeamTracker>();
    }

    virtual public void Start() {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0 && !Melded)
        {
            Die();
        }
    }

    /// <summary>
    /// Destroy the cell.
    /// </summary>
    public void Die()
    {
        
        if (_teamTracker.Team == Team.Enemy) {
            SignalEnemyDeath?.Invoke();
            // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
            bool convertCell = UnityEngine.Random.Range(0f, 1f) < ConvertChance;
            if (convertCell) {
                NeutralizeCell();
                AudioManager.Instance.PlayUISound(1.4f);
            } else {
                PlayDeathFX();
                Destroy(gameObject);
                if (UnityEngine.Random.Range(0f, 1f) < CoinDropChance) {
                    //Instantiate(PrefabsManager.Instance.Coin, transform.position, Quaternion.identity, transform.parent);
                }
                BeingDestroyed = true;
            }
        } else {
            PlayDeathFX();
            Destroy(gameObject);
            BeingDestroyed = true;
        }
    }

    public void PlayDeathFX()
    {
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake();
    }

    public virtual void NeutralizeCell()
    {
        if (_teamTracker.Team != Team.Enemy)
        {
            Debug.LogWarning("Tried to convert a cell that was not an enemy");
        }
        gameObject.layer = Layers.NeutralCell;
        _teamTracker.ChangeTeam(Team.Neutral);
        
        CurrentHealth = MaxHealth;
    }

    public void Meld() {
        Melded = true;
    }

    public void OnCollisionEnter(Collision collision) {
        // Only handle bullet collisions.
        // Furthhermore, only handle bullet collisions for enemies. Player bullet collisions
        // will be handled in the Player object.
        if (_teamTracker.Team == Team.Enemy && collision.gameObject.layer == Layers.PlayerBullet) {
            Die();
        }
    }
}
