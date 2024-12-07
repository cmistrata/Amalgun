using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CellHealthManager : MonoBehaviour {
    public static event Action SignalEnemyCellDefeat;
    public static event Action<GameObject> SignalPlayerCellDeath;


    private Cell _stateTracker;
    private Animator _animator;
    private bool _isPlayerHealthManager;

    [Header("Health info")]
    public int MaxHealth = 1;
    public int CurrentHealth = 1;

    [Range(0, 1)]
    [Header("Conversion RNG")]
    public float ConvertChance = 0.15f;

    public void Awake() {
        _stateTracker = GetComponent<Cell>();
        _animator = GetComponent<Animator>();
        _isPlayerHealthManager = GetComponent<Player>() != null;
    }

    virtual public void Start() {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damage = 1) {
        if (CurrentHealth <= 0) {
            Debug.LogWarning($"Cell {gameObject} in state {_stateTracker.State} took damage but does not have any health.");
            return;
        }

        CurrentHealth -= damage;
        if (_animator != null) {
            _animator.SetTrigger("Hit");
        }
        if (CurrentHealth <= 0) {
            Die();
        }
    }

    /// <summary>
    /// Kills the cell. Returns true if the cell is being destroyed
    /// </summary>
    public void Die() {
        switch (_stateTracker.State) {
            case CellState.Player:
                SignalPlayerCellDeath?.Invoke(gameObject);
                PlayDeathFX();
                Destroy(gameObject);
                break;
            case CellState.Enemy:
                SignalEnemyCellDefeat?.Invoke();
                // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
                bool convertCell = UnityEngine.Random.Range(0f, 1f) < ConvertChance;
                if (convertCell) {
                    NeutralizeCell();
                }
                else {
                    Destroy(gameObject);
                    PlayDeathFX();
                }
                break;
        }
    }

    public void PlayDeathFX() {
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake();
        EffectsManager.InstantiateEffect(Effect.ToonExplosion, transform.position);
    }

    public virtual void NeutralizeCell() {
        if (_stateTracker.State != CellState.Enemy) {
            Debug.LogWarning("Tried to convert a cell that was not an enemy");
        }
        AudioManager.Instance.PlayNeutralizeSound(1.4f);
        gameObject.layer = Layers.NeutralCell;
        gameObject.transform.parent = Containers.Cells;
        _stateTracker.ChangeState(CellState.Neutral);

        CurrentHealth = MaxHealth;
    }

    public void Meld() {
        _stateTracker.ChangeState(CellState.PlayerMelded);
    }

    public void OnCollisionEnter(Collision collision) {
        if (_isPlayerHealthManager) {
            // Player script handles collisions.
            return;
        }
        bool isOtherStateBullet = (_stateTracker.State == CellState.Enemy && collision.gameObject.layer == Layers.PlayerBullet)
               || (_stateTracker.State == CellState.Player && collision.gameObject.layer == Layers.EnemyBullet);
        if (isOtherStateBullet) {
            TakeDamage(1);
        }
    }
}
