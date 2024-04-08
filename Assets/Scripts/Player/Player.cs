using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Mover))]
public class Player : MonoBehaviour {
    public int MaxHealth = 4;
    public int CurrentHealth = 4;
    private Rigidbody _rb;
    private Mover _mover;

    [SerializeField]
    private Dictionary<GameObject, List<GameObject>> _cellGraph = new();
    // Wait a bit to connect new cells to allow sliding them into place.
    private const float _timeToConnect = .03f;
    private Dictionary<GameObject, float> _connectionTimeByCell = new();
    public static Player Instance;

    private float _rotationalInertia = 100f;
    private const float _torque = 5000f;
    private const float _newCellMassIncrease = 15f;

    public static event Action SignalPlayerDeath;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _rb.inertiaTensor = new(0, _rotationalInertia, 0);
        _mover = GetComponent<Mover>();
        _cellGraph.Add(gameObject, new List<GameObject>());
        Instance = this;
    }

    // Update is called once per frame
    void Update() {
        DetectAndHandleClick();
    }

    private void FixedUpdate() {
        Vector3 newTargetDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _mover.TargetDirection = newTargetDirection.normalized;

        float clockwiseRotationInput = Input.GetAxis("Rotate Clockwise");
        _rb.AddTorque(Vector3.up * _torque * clockwiseRotationInput);
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
    /// Adds a cell to the player and updates the cell graph
    /// </summary>
    public void ConnectCell(GameObject neutralCell) {
        List<GameObject> cellsTouchingPlayer = FindCellsTouchingPlayerAndDestroyNearbyBullets(neutralCell);

        // Add the new cell to the graph, and simultaneously create edges
        // from it to all the cells near it during the collision.
        _cellGraph.Add(neutralCell, cellsTouchingPlayer);
        _rb.mass += _newCellMassIncrease;
        neutralCell.transform.parent = transform;
        // Also create edges from its neighbors to it.
        foreach (GameObject cellTouchingPlayer in cellsTouchingPlayer) {
            try {
                _cellGraph[cellTouchingPlayer].Add(neutralCell);
            } catch (KeyNotFoundException) {
                Debug.LogError($"Tried to update adjacencies for cell ${cellTouchingPlayer} not in the Player's cellGraph.");
            }
        }

        // Remove the cell's RigidBody, but not its collider.
        // This will effectively combine the cell's collider into the player's.
        neutralCell.GetComponent<Mover>().enabled = false;
        Destroy(neutralCell.GetComponent<Rigidbody>());
        if (neutralCell.TryGetComponent<StayInBounds>(out var stayInBounds)) {
            stayInBounds.enabled = false;
        }

        // Make the cell's Team "Team.Player".
        neutralCell.GetComponent<TeamTracker>().ChangeTeam(Team.Player);

        UpdateRotationalInertia();
    }

    private List<GameObject> FindCellsTouchingPlayerAndDestroyNearbyBullets(GameObject neutralCell) {
        Collider[] nearbyColliders = Physics.OverlapSphere(neutralCell.transform.position, .525f);
        List<GameObject> adjacentCells = new();

        foreach (Collider nearbyCollider in nearbyColliders) {
            if (nearbyCollider.transform == neutralCell.transform) continue;

            var nearbyObject = nearbyCollider.gameObject;
            var nearbyObjectIsPlayerCell = _cellGraph.ContainsKey(nearbyObject);
            if (nearbyObjectIsPlayerCell) {
                adjacentCells.Add(nearbyObject);
            }

            // Destroy nearby bullets on attach to prevent instafrag
            if (nearbyObject.TryGetComponent<Bullet>(out var _nearbyBullet)) {
                Destroy(nearbyObject);
            }
        }

        return adjacentCells;
    }

    /// <summary>
    /// Removes a cell from the player and updates the cell graph
    /// Then checks the connection and removes other detached cells
    /// </summary>
    public void DamageCell(GameObject cell) {
        var cellComponent = cell.GetComponent<CellHealthManager>();
        cellComponent.TakeDamage(1);
        if (cellComponent.BeingDestroyed) {
            DisconnectCell(cell);
        }
    }

    public void DisconnectCell(GameObject cell) {
        // Decrease mass by the cell's mass
        if (_rb.mass > 1) {
            _rb.mass -= _newCellMassIncrease;
        }

        // Remove any edges containing the cell, and then remove the node representing the cell
        foreach (GameObject p in _cellGraph.Keys) {
            _cellGraph[p].Remove(cell);
        }
        _cellGraph.Remove(cell);
        cell.transform.parent = null;
        DestroyUnconnectedCells();
        UpdateRotationalInertia();
    }

    private void DestroyUnconnectedCells() {
        var cellsConnectedToPlayer = GetCellsConnectedToPlayer();
        var newlyDisconnectedCells = _cellGraph.Keys.Where(cell => !cellsConnectedToPlayer.Contains(cell)).ToList();

        foreach (GameObject newlyDisconnectedCell in newlyDisconnectedCells) {
            _cellGraph.Remove(newlyDisconnectedCell);
            _rb.mass -= _newCellMassIncrease;
            newlyDisconnectedCell.GetComponent<CellHealthManager>().Die();
        }
    }

    private List<GameObject> GetCellsConnectedToPlayer() {
        List<GameObject> connectedCells = new() { gameObject };
        Stack<GameObject> cellsToSearch = new();
        cellsToSearch.Push(gameObject);

        while (cellsToSearch.Count > 0) {
            var currentCell = cellsToSearch.Pop();
            if (currentCell == null || !_cellGraph.ContainsKey(currentCell)) {
                Debug.LogError($"Couldn't find {currentCell} in cell graph.");
                continue;
            }
            var neighbors = _cellGraph[currentCell];
            foreach (GameObject p in neighbors) {
                if (!connectedCells.Contains(p)) {
                    cellsToSearch.Push(p);
                    connectedCells.Add(p);
                }
            }
        }

        return connectedCells;
    }

    public void DestroyAllPlayerCells() {
        foreach (GameObject cell in _cellGraph.Keys) {
            cell.GetComponent<CellHealthManager>().PlayDeathFX();
        }
        _cellGraph = new();
    }

    private string GetCellGraphAsString() {
        var printString = "";
        foreach (GameObject cell in _cellGraph.Keys) {
            printString += $"{cell.name}: {String.Join(", ", _cellGraph[cell])}\n";
        }
        return printString;
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

    private float UpdateRotationalInertia() {
        var rotationalInertia = 100f;
        foreach (var cell in _cellGraph.Keys) {
            rotationalInertia += (cell.transform.position - transform.position).sqrMagnitude * 15f;
        }
        _rb.inertiaTensor = new(0, rotationalInertia, 0);
        return rotationalInertia;
    }
}
