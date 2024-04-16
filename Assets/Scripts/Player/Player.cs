using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour {
    public int MaxHealth = 4;
    public int CurrentHealth = 4;
    private Rigidbody _rb;
    private PlayerShip _playerShip;

    // Wait a bit to connect new cells to allow sliding them into place.
    private const float _timeToConnect = .03f;
    private Dictionary<GameObject, float> _connectionTimeByCell = new();
    public static Player Instance;

    private const float _torque = 5000f;
    private const float _newCellMassIncrease = 15f;

    public static event Action SignalPlayerDeath;

    private void Awake()
    {
        Instance = this;

        _playerShip = new PlayerShip(gameObject);

        _rb = GetComponent<Rigidbody>();
        UpdateRotationalInertia();
    }

    // Update is called once per frame
    void Update() {
        DetectAndHandleClick();
    }

    void DetectAndHandleClick() {
        if (Input.GetMouseButtonDown(0) && Utils.MouseRaycast(out var hit)) {
            if (GameManager.Instance.State == GameState.Shop) {
                //if (GameManager.Instance.Money <= 0) return;
                var clickedGameObject = hit.collider.gameObject;
                if (clickedGameObject == this.gameObject) {
                    GameManager.Instance.SpendMoney();
                    Heal(1);
                } else if (clickedGameObject.GetComponent<CellHealthManager>() != null) {
                    var clickedCell = clickedGameObject.GetComponent<CellHealthManager>();
                    GameManager.Instance.SpendMoney();
                    clickedCell.Meld();
                }
            }
        }
    }

    public void Heal(int amount) {
        CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
    }

    public void TakeDamage(int damage) {
        CurrentHealth = Math.Max(CurrentHealth - damage, 0);
        if (CurrentHealth <= 0) {
            Die();
            return;
        }

        AudioManager.Instance.PlayPlayerDamagedSound(1 + (4f - CurrentHealth) / 8f);
        CameraManager.Instance.FlashDamageFilter();
    }


    /// <summary>
    /// Adds a cell to the player and updates the ship
    /// </summary>
    public void ConnectCell(GameObject neutralCell) {
        _playerShip.ConnectCell(neutralCell);

        _rb.mass += _newCellMassIncrease;
        neutralCell.transform.parent = transform;

        // Remove the cell's RigidBody, but not its collider.
        // This will effectively combine the cell's collider into the player's.
        Destroy(neutralCell.GetComponent<Rigidbody>());
        if (neutralCell.TryGetComponent<StayInBounds>(out var stayInBounds)) {
            stayInBounds.enabled = false;
        }

        // Make the cell's Team "Team.Player".
        neutralCell.GetComponent<TeamTracker>().ChangeTeam(Team.Player);

        UpdateRotationalInertia();
    }

    /// <summary>
    /// Removes a cell from the player and updates the ship
    /// </summary>
    public void DamageCell(GameObject cell) {
        var cellComponent = cell.GetComponent<CellHealthManager>();
        bool destroyed = cellComponent.TakeDamage(1);
        if (destroyed) 
        {
            var disconnectedCells = _playerShip.DisconnectCell(cell);
            foreach (var diconnectedCell in disconnectedCells)
            {
                var disonnectedCellComponent = diconnectedCell.GetComponent<CellHealthManager>();
                disonnectedCellComponent.Die();

                // Decrease mass by the disconnected cell's mass
                if (_rb.mass > 1)
                {
                    _rb.mass -= _newCellMassIncrease;
                }
            }
            UpdateRotationalInertia();
        }
    }

    private void Die() {
        PlayDeathFX();
        Destroy(gameObject);
        SignalPlayerDeath?.Invoke();
    }

    public void HandleBulletCollision(Collision collision) {
        var nonBulletObject = collision.GetContact(0).thisCollider.gameObject;
        var collisionIsWithPlayerShip = nonBulletObject == gameObject;
        if (collisionIsWithPlayerShip) {
            TakeDamage(1);
        } else {
            DamageCell(nonBulletObject);
        }
    }

    void OnCollisionEnter(Collision other) {
        int collisionLayer = other.gameObject.layer;
        switch (collisionLayer) {
            case Layers.NeutralCell:
                _connectionTimeByCell[other.gameObject] = Time.time + _timeToConnect;
                break;
            case Layers.EnemyBullet:
                HandleBulletCollision(other);
                break;
        }
    }

    private void OnCollisionStay(Collision other) {
        int collisionLayer = other.gameObject.layer;
        if (collisionLayer == Layers.NeutralCell) {
            if (Time.time >= _connectionTimeByCell[other.gameObject]) {
                ConnectCell(other.gameObject);
                _connectionTimeByCell.Remove(other.gameObject);
            }
        }
    }

    public void PlayDeathFX() {
        //Instantiate(PrefabsManager.Instance.PlayerDeathEffect, transform.position, Quaternion.identity, transform.parent);
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake(1, .6f);
    }

    private void UpdateRotationalInertia() {
        _rb.inertiaTensor = new(0, _playerShip.GetRotationalInertia(), 0);
    }
}
