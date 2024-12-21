using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CellHealthManager : MonoBehaviour {
    public static event Action<GameObject> SignalEnemyCellDefeat;
    public static event Action<GameObject> SignalPlayerCellDeath;


    private Cell _stateTracker;
    private Animator _animator;

    [Header("Health info")]
    public int MaxHealth = 1;
    public int CurrentHealth = 1;

    [Range(0, 1)]
    [Header("Conversion RNG")]
    public float ConvertChance = 0.15f;

    public void Awake() {
        _stateTracker = GetComponent<Cell>();
        _animator = GetComponent<Animator>();
    }

    virtual public void Start() {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(int damage = 1) {
        if (CurrentHealth <= 0) {
            Debug.LogWarning($"Cell {gameObject} in state {_stateTracker.State} took damage but does not have any health.");
            return;
        }

        if (_animator != null) {
            _animator.SetTrigger("Hit");
        }
        if (_stateTracker.State == CellState.Melded && gameObject != GameManager.Instance.Player) {
            if (GameManager.Instance == null || GameManager.Instance.Player == null) {
                Debug.LogError($"Melded cell {gameObject} took damage, but player isn't tracked in game manager.");
            }
            GameManager.Instance.Player.GetComponent<Player>().TakeDamage();
        }
        else {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0) {
                Defeat();
            }
        }
    }

    /// <summary>
    /// Kills the cell. Returns true if the cell is being destroyed
    /// </summary>
    public void Defeat() {
        switch (_stateTracker.State) {
            case CellState.Friendly:
                SignalPlayerCellDeath?.Invoke(gameObject);
                Destruct();
                break;
            case CellState.Enemy:
                bool convertCell = GameManager.Instance.AutoKoEnemies || GameManager.EnemiesLeftInWave() <= 1 || UnityEngine.Random.Range(0f, 1f) < ConvertChance;
                SignalEnemyCellDefeat?.Invoke(gameObject);
                // Convert an enemy with a given chance, or auto convert if it is the last enemy in the wave
                if (convertCell) {
                    NeutralizeCell();
                    PlayNeutralizeFx();
                }
                else {
                    Destruct();
                }
                break;
        }
    }

    public void Destruct() {
        PlayDeathFX();
        Destroy(gameObject);
    }

    public void PlayDeathFX() {
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake();
        EffectsManager.InstantiateEffect(Effect.ToonExplosion, transform.position);
    }

    public void PlayNeutralizeFx() {
        AudioManager.Instance.PlayNeutralizeSound(1.4f);
        EffectsManager.InstantiateEffect(Effect.KnockoutHit, transform.position);
    }

    public virtual void NeutralizeCell() {
        if (_stateTracker.State != CellState.Enemy) {
            Debug.LogWarning("Tried to convert a cell that was not an enemy");
        }

        gameObject.layer = Layers.NeutralCell;
        gameObject.transform.parent = Containers.Cells;
        _stateTracker.ChangeState(CellState.Neutral);

        CurrentHealth = MaxHealth;
    }

    public void Meld() {
        _stateTracker.ChangeState(CellState.Melded);
    }

    // Note: collisions for rigidbodyless colliders under a rigidbodyfull parent (friendly cells under player)
    // will have the OnCollisionEnter event be triggered on the parent.
    public void OnCollisionEnter(Collision collision) {
        bool isOtherStateBullet = (_stateTracker.State == CellState.Enemy && collision.gameObject.layer == Layers.PlayerBullet)
               || (_stateTracker.State == CellState.Friendly && collision.gameObject.layer == Layers.EnemyBullet);
        if (isOtherStateBullet) {
            TakeDamage(1);
        }
    }
}
