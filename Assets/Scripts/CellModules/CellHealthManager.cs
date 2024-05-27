using System;
using UnityEngine;

public enum Team {
    Player,
    Enemy,
    Neutral
}

[RequireComponent(typeof(AudioSource))]
public class CellHealthManager : MonoBehaviour {
    public static event Action SignalEnemyCellDeath;
    public static event Action<GameObject> SignalPlayerCellDeath;


    private TeamTracker _teamTracker;
    private Animator _animator;

    [Header("Health info")]
    public int MaxHealth = 1;
    public int CurrentHealth = 1;

    [Range(0, 1)]
    [Header("Conversion RNG")]
    public float ConvertChance = 0.15f;
    public bool Melded = false;

    public void Awake() {
        _teamTracker = GetComponent<TeamTracker>();
        _animator = GetComponent<Animator>();
    }

    virtual public void Start() {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damage = 1) {
        if (_animator != null) {
            _animator.SetTrigger("Hit");
        }

        CurrentHealth -= damage;
        if (CurrentHealth <= 0) {
            Die();
        }
    }

    /// <summary>
    /// Kills the cell. Returns true if the cell is being destroyed
    /// </summary>
    public void Die() {
        switch (_teamTracker.Team) {
            case Team.Player:
                Debug.Log($"Killing player cell {gameObject.name}");
                SignalPlayerCellDeath?.Invoke(gameObject);
                PlayDeathFX();
                Destroy(gameObject);
                break;
            case Team.Enemy:
                SignalEnemyCellDeath?.Invoke();
                // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
                bool convertCell = UnityEngine.Random.Range(0f, 1f) < ConvertChance;
                if (convertCell) {
                    NeutralizeCell();
                    break;
                }

                Destroy(gameObject);
                PlayDeathFX();
                break;
            default:
                break;
        }
    }

    public void PlayDeathFX() {
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake();
        EffectsManager.InstantiateEffect(Effect.ToonExplosion, transform.position);
    }

    public virtual void NeutralizeCell() {
        if (_teamTracker.Team != Team.Enemy) {
            Debug.LogWarning("Tried to convert a cell that was not an enemy");
        }
        AudioManager.Instance.PlayUISound(1.4f);
        gameObject.layer = Layers.NeutralCell;
        _teamTracker.ChangeTeam(Team.Neutral);

        CurrentHealth = MaxHealth;
    }

    public void Meld() {
        Melded = true;
    }

    public void OnCollisionEnter(Collision collision) {
        bool isOtherTeamBullet = (_teamTracker.Team == Team.Enemy && collision.gameObject.layer == Layers.PlayerBullet)
               || (_teamTracker.Team == Team.Player && collision.gameObject.layer == Layers.EnemyBullet);
        if (isOtherTeamBullet) {
            TakeDamage(1);
        }
    }
}
