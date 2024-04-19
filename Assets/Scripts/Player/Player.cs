using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour {
    private Rigidbody _rb;
    private PlayerShip _playerShip;
    private GameObject _nucleus;

    // Wait a bit to connect new cells to allow sliding them into place.
    private const float _timeToConnect = .03f;
    private Dictionary<GameObject, float> _connectionTimeByCell = new();
    public static Player Instance;

    private const float _torque = 5000f;
    private const float _newCellMassIncrease = 15f;

    public static event Action SignalPlayerDeath;

    private void Awake()
    {
        foreach (var cellProperties in GetComponentsInChildren<CellProperties>())
        {
            if (cellProperties.Type == CellType.PlayerNucleus)
            {
                _nucleus = cellProperties.gameObject;
                break;
            }
        }
        if (_nucleus == null)
        {
            Debug.LogError("Failed to find player nucleus cell for player");
        }

        _playerShip = new PlayerShip(_nucleus);
        Instance = this;


        _rb = GetComponent<Rigidbody>();
        UpdateRotationalInertia();

        CellHealthManager.SignalPlayerCellDeath += OnPlayerCellDeath;
        PlayerShipPiece.SignalAttachCell += ConnectCell;
    }

    private void OnDestroy()
    {
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
        //CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
    }

    /// <summary>
    /// Adds a cell to the player and updates the ship
    /// </summary>
    public void ConnectCell(GameObject neutralCell)
    {
        neutralCell.GetComponent<TeamTracker>().ChangeTeam(Team.Player);

        ModifyCellToFollowShip(neutralCell);
        _playerShip.ConnectCell(neutralCell);
        _rb.mass += _newCellMassIncrease;

        UpdateRotationalInertia();
        neutralCell.GetComponent<PlayerShipPiece>().enabled = true;
    }

    public void DisconnectCell(GameObject playerCell)
    {
        RestoreCellOnRemovalFromShip(playerCell);
        _playerShip.RemoveCell(playerCell);
        _rb.mass -= _newCellMassIncrease;

        UpdateRotationalInertia();
        playerCell.GetComponent<PlayerShipPiece>().enabled = false;
    }

    private void ModifyCellToFollowShip(GameObject cell)
    {
        cell.transform.parent = transform;
        var rb = cell.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        if (cell.TryGetComponent<StayInBounds>(out var stayInBounds))
        {
            stayInBounds.enabled = false;
        }
    }

    private void RestoreCellOnRemovalFromShip(GameObject cell)
    {
        cell.transform.parent = null; //TODO: where should it be parented?
        Rigidbody rb = cell.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        if (cell.TryGetComponent<StayInBounds>(out var stayInBounds))
        {
            stayInBounds.enabled = true;
        }
    }

    private void OnPlayerCellDeath(GameObject cell)
    {
        if (cell == _nucleus)
        {
            Die();
            return;
        }

        DisconnectCell(cell);
        foreach (var diconnectedCell in _playerShip.GetAndRemoveDisonnectedCells())
        {
            DisconnectLiveCell(diconnectedCell);
        }
    }

    private void DisconnectLiveCell(GameObject cell)
    {
        cell.GetComponent<CellHealthManager>().Die();
        //TODO: possible different behavior for a cell that was incidentally disconnected, become neutral?
    }

    private void Die() 
    {
        PlayDeathFX();
        gameObject.SetActive(false);
        SignalPlayerDeath?.Invoke();
    }

    public void PlayDeathFX() 
    {
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake(1, .6f);
    }

    private void UpdateRotationalInertia() 
    {
        _rb.inertiaTensor = new(0, _playerShip.GetRotationalInertia(), 0);
    }
}
