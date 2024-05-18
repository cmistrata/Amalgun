using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CellHealthManager))]
public class Player : MonoBehaviour {
    private Rigidbody _rb;
    private CellHealthManager _cellHealthManager;
    private PlayerShip _playerShip;
    private GameObject _nucleus;

    // Wait a bit to connect new cells to allow sliding them into place.
    private const float _timeToConnect = .03f;
    private Dictionary<GameObject, float> _connectionTimeByCell = new();
    public static Player Instance;

    private const float _torque = 5000f;
    private const float _newCellMassIncrease = 30f;

    public static event Action SignalPlayerDeath;

    private float _currentHealth = 4;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _cellHealthManager = GetComponent<CellHealthManager>();
        _nucleus = gameObject;

        _playerShip = new PlayerShip(_nucleus);
        Instance = this;

        UpdateRotationalInertia();

        CellHealthManager.SignalPlayerCellDeath += OnPlayerCellDeath;
        PlayerShipPiece.SignalAttachCell += ConnectCell;
    }

    private void OnDestroy() {
        CellHealthManager.SignalPlayerCellDeath -= OnPlayerCellDeath;
        PlayerShipPiece.SignalAttachCell -= ConnectCell;
    }

    // Update is called once per frame
    void Update() {
        DetectAndHandleClick();
    }

    void DetectAndHandleClick() {
        if (Input.GetMouseButtonDown(0) && Utils.MouseRaycast(out var hit)) {
            if (GameManager.Instance.State == GameState.Shop) {
                var clickedGameObject = hit.collider.gameObject;
                if (clickedGameObject == this.gameObject) {
                    Heal(1);
                }
                else if (clickedGameObject.GetComponent<CellHealthManager>() != null) {
                    var clickedCell = clickedGameObject.GetComponent<CellHealthManager>();
                    clickedCell.Meld();
                }
            }
        }
    }

    public void Heal(int amount) {
        //CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
    }

    /// <summary>
    /// Adds a cell to the player and updates the ship
    /// </summary>
    public void ConnectCell(GameObject neutralCell) {
        neutralCell.GetComponent<TeamTracker>().ChangeTeam(Team.Player);
        Destroy(neutralCell.GetComponent<Rigidbody>());

        ModifyCellToFollowShip(neutralCell);
        _playerShip.ConnectCell(neutralCell);
        _rb.mass += _newCellMassIncrease;

        UpdateRotationalInertia();
        neutralCell.GetComponent<PlayerShipPiece>().enabled = true;
    }

    public void DisconnectCell(GameObject playerCell) {
        playerCell.transform.parent = null;
        _playerShip.RemoveCell(playerCell);
        _rb.mass -= _newCellMassIncrease;

        UpdateRotationalInertia();
        playerCell.GetComponent<PlayerShipPiece>().enabled = false;
    }

    private void ModifyCellToFollowShip(GameObject cell) {
        cell.transform.parent = transform;
        var rb = cell.GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    // private void RestoreCellOnRemovalFromShip(GameObject cell) {
    //     cell.transform.parent = null; //TODO: where should it be parented?
    //     Rigidbody rb = cell.GetComponent<Rigidbody>();
    //     rb.isKinematic = false;

    //     if (cell.TryGetComponent<StayInBounds>(out var stayInBounds)) {
    //         stayInBounds.enabled = true;
    //     }
    // }

    private void OnPlayerCellDeath(GameObject cell) {
        if (cell == gameObject) {
            Die();
            return;
        }

        DisconnectCell(cell);
        foreach (var disconnectedCell in _playerShip.GetAndRemoveDisconnectedCells()) {
            DisconnectLiveCell(disconnectedCell);
        }
    }

    private void DisconnectLiveCell(GameObject cell) {
        cell.GetComponent<CellHealthManager>().Die();
        //TODO: possible different behavior for a cell that was incidentally disconnected, become neutral?
    }

    private void Die() {
        PlayDeathFX();
        gameObject.SetActive(false);
        SignalPlayerDeath?.Invoke();
    }

    public void PlayDeathFX() {
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake(1, .6f);
    }

    private void UpdateRotationalInertia() {
        _rb.inertiaTensor = new(0, _playerShip.GetRotationalInertia(), 0);
    }

    void OnCollisionEnter(Collision collision) {
        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer != Layers.EnemyBullet) return;

        var nonBulletObject = collision.GetContact(0).thisCollider.gameObject;
        var collisionIsWithPlayerShip = nonBulletObject == gameObject;
        if (collisionIsWithPlayerShip) {
            TakeDamage();
        }
        else if (nonBulletObject.TryGetComponent<CellHealthManager>(out var childCellHealthManager)) {
            childCellHealthManager.TakeDamage();
        }
    }

    public void TakeDamage() {
        AudioManager.Instance.PlayPlayerDamagedSound(1 + (4f - _cellHealthManager.CurrentHealth) / 8f);
        CameraManager.Instance.FlashDamageFilter();
        _cellHealthManager.TakeDamage();
    }
}
