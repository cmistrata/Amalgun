using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Amalgamator : MonoBehaviour {
    const float _timeToConnect = .01f;
    const float _connectionRadius = .51f;
    const float _baseRotationalInertia = 100f;
    Dictionary<GameObject, HashSet<GameObject>> _cellGraph;
    Rigidbody _rb;
    private Dictionary<GameObject, float> _connectionTimeByCell = new();
    void Awake() {
        _cellGraph = new() {
            [gameObject] = new()
        };
        _rb = GetComponent<Rigidbody>();
        CellHealthManager.SignalPlayerCellDeath += HandlePlayerCellDeath;
    }

    void Connect(GameObject cell) {
        var touchingCells = FindTouchingCells(cell);
        foreach (var touchingCell in touchingCells) {
            Connect(cell, touchingCell);
        }
        UpdatePhysicalProperties();
    }

    void Connect(GameObject cell1, GameObject cell2) {
        if (!_cellGraph.ContainsKey(cell1)) {
            _cellGraph[cell1] = new();
            if (cell1.TryGetComponent<ConnectHandler>(out var connectHandler1)) {
                connectHandler1.Connect(gameObject);
            }
        }
        if (!_cellGraph.ContainsKey(cell2)) {
            _cellGraph[cell2] = new();
            if (cell2.TryGetComponent<ConnectHandler>(out var connectHandler2)) {
                connectHandler2.Connect(gameObject);
            }
        }
        _cellGraph[cell1].Add(cell2);
        _cellGraph[cell2].Add(cell1);
    }

    void Disconnect(GameObject cell, bool destroyed) {
        RemoveCell(cell, destroyed);

        var disconnectedCells = GetUnconnectedCells();
        foreach (var disconnectedCell in disconnectedCells) {
            RemoveCell(disconnectedCell, false);
        }
        UpdatePhysicalProperties();
    }

    void RemoveCell(GameObject cell, bool destroyed) {
        if (cell == gameObject) {
            Debug.LogError("Tried disconnecting the center of an amalgamator.");
            return;
        }
        foreach (GameObject neighbor in _cellGraph[cell]) {
            _cellGraph[neighbor].Remove(cell);
        }
        if (!destroyed) {
            cell.GetComponent<ConnectHandler>().Disconnect(_rb);
        }
        _cellGraph.Remove(cell);
    }

    private HashSet<GameObject> FindTouchingCells(GameObject cell) {
        HashSet<GameObject> adjacentCells = new();

        Collider[] nearbyColliders = Physics.OverlapSphere(cell.transform.position, _connectionRadius, Utils.GetLayerMask(Layers.PlayerCell));
        foreach (Collider nearbyCollider in nearbyColliders) {
            GameObject nearbyObject = nearbyCollider.gameObject;

            if (nearbyObject == this) continue;
            if (_cellGraph.ContainsKey(nearbyObject)) {
                adjacentCells.Add(nearbyObject);
            }
        }

        return adjacentCells;
    }

    private HashSet<GameObject> GetConnectedCells() {
        HashSet<GameObject> visitedCells = new() { gameObject };
        Stack<GameObject> cellsToVisit = new();
        cellsToVisit.Push(gameObject);

        while (cellsToVisit.Any()) {
            var currentCell = cellsToVisit.Pop();
            foreach (GameObject neighbor in _cellGraph[currentCell]) {
                if (!visitedCells.Contains(neighbor)) {
                    visitedCells.Add(neighbor);
                    cellsToVisit.Push(neighbor);
                }
            }
        }

        return visitedCells;
    }

    private HashSet<GameObject> GetUnconnectedCells() {
        return _cellGraph.Keys.Except(GetConnectedCells()).ToHashSet();
    }

    private void UpdatePhysicalProperties() {
        UpdateMass();
        UpdateRotationalInertia();
    }

    private void UpdateMass() {
        float mass = 100f + 30f * (_cellGraph.Count - 1);
        _rb.mass = mass;
    }

    private void UpdateRotationalInertia() {
        float rotationalInertia = _baseRotationalInertia;
        foreach (GameObject cell in _cellGraph.Keys) {
            rotationalInertia += (cell.transform.position - transform.position).sqrMagnitude * 15f;
        }
        _rb.inertiaTensor = new(0, rotationalInertia, 0);
    }

    public void OnCollisionEnter(Collision collision) {
        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer == Layers.NeutralCell) {
            _connectionTimeByCell[collision.gameObject] = Time.time + _timeToConnect;
        }
    }

    public void OnCollisionStay(Collision collision) {
        if (enabled == false) return;
        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer != Layers.NeutralCell) return;

        if (!_connectionTimeByCell.ContainsKey(collision.gameObject)) {
            _connectionTimeByCell[collision.gameObject] = Time.time + _timeToConnect;
        }
        if (Time.time >= _connectionTimeByCell[collision.gameObject]) {
            Connect(collision.gameObject);
            _connectionTimeByCell.Remove(collision.gameObject);
        }
    }

    public void HandlePlayerCellDeath(GameObject cell) {
        Disconnect(cell, destroyed: true);
    }

    public void PrintCellGraph() {
        string printStr = "Cell graph: {";
        foreach (GameObject cell in _cellGraph.Keys) {
            printStr += $"{cell.name}: [";
            foreach (GameObject neighbor in _cellGraph[cell]) {
                printStr += $"{neighbor.name},";
            }
            printStr += "],";
        }
        printStr += "}";
        Debug.Log(printStr);
    }
}
