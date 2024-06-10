using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Amalgamator : MonoBehaviour {
    const float _timeToConnect = .03f;
    const float _connectionRadius = .53f;
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

    void OnDestroy() {
        CellHealthManager.SignalPlayerCellDeath -= HandlePlayerCellDeath;
    }


    /// <summary>
    /// Try to connect a cell, possibly merging it, and doing nothing if there aren't other cells nearby.
    /// </summary>
    void TryAmalgamate(GameObject cell) {
        var cellType = cell.GetComponent<CellProperties>().Type;
        var touchingCells = FindTouchingCells(cell);
        // Try merging if possible.
        if (Globals.CellUpgradeByType.ContainsKey(cellType)) {
            foreach (var secondaryCell in touchingCells) {
                if (secondaryCell == gameObject || secondaryCell.GetComponent<CellProperties>().Type != cellType) continue;
                var tertiaryCells = GetLinkedCells(secondaryCell);
                tertiaryCells.UnionWith(touchingCells);
                tertiaryCells.Remove(secondaryCell);
                foreach (var tertiaryCell in tertiaryCells) {
                    if (tertiaryCell == gameObject || tertiaryCell.GetComponent<CellProperties>().Type != cellType) continue;
                    Merge(incomingCell: cell, secondaryCell: secondaryCell, tertiaryCell: tertiaryCell, cellType);
                    return;
                }
            }
        }

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

    HashSet<GameObject> GetLinkedCells(GameObject cell) {
        if (!_cellGraph.ContainsKey(cell)) {
            Debug.LogWarning($"Tried getting connected cells for cell {cell} not in graph.");
            return new HashSet<GameObject>();
        }
        return _cellGraph[cell];
    }

    void Merge(GameObject incomingCell, GameObject secondaryCell, GameObject tertiaryCell, CellType cellType) {
        Destroy(incomingCell);

        var secondaryCellDistance = (secondaryCell.transform.position - transform.position).sqrMagnitude;
        var tertiaryCellDistance = (tertiaryCell.transform.position - transform.position).sqrMagnitude;
        Vector3 mergedCellPosition;
        Quaternion mergedCellRotation;
        // Replace tertiary cell with merged cell
        if (secondaryCellDistance > tertiaryCellDistance) {
            tertiaryCell.transform.GetPositionAndRotation(out mergedCellPosition, out mergedCellRotation);

        }
        // Replace secondary cell with merged cell
        else {
            secondaryCell.transform.GetPositionAndRotation(out mergedCellPosition, out mergedCellRotation);
        }
        Destroy(secondaryCell);
        Disconnect(secondaryCell, destroyed: true);
        Destroy(tertiaryCell);
        Disconnect(tertiaryCell, destroyed: true);

        var newCellType = Globals.CellUpgradeByType[cellType];
        var newCellPrefab = Globals.CellPrefabByType[newCellType];
        GameObject newCell = Instantiate(newCellPrefab, position: mergedCellPosition, rotation: mergedCellRotation, parent: Containers.Cells);
        TryAmalgamate(newCell);
    }

    void RemoveCell(GameObject cell, bool destroyed) {
        if (cell == gameObject) {
            Debug.LogError("Tried disconnecting the center of an amalgamator.");
            return;
        }
        if (!_cellGraph.ContainsKey(cell)) {
            return;
        }
        foreach (GameObject neighbor in GetLinkedCells(cell)) {
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
            foreach (GameObject neighbor in GetLinkedCells(currentCell)) {
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
            PrintCellGraph();
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
            TryAmalgamate(collision.gameObject);
            _connectionTimeByCell.Remove(collision.gameObject);
        }
    }

    public void HandlePlayerCellDeath(GameObject cell) {
        if (cell == gameObject) {
            foreach (var secondaryCell in new HashSet<GameObject>(GetLinkedCells(gameObject))) {
                Disconnect(secondaryCell, false);
            }
        }
        else {
            Disconnect(cell, destroyed: true);
        }
    }

    public void PrintCellGraph() {
        string printStr = "Cell graph: {";
        foreach (GameObject cell in _cellGraph.Keys) {
            printStr += $"{cell.name}: [";
            foreach (GameObject neighbor in GetLinkedCells(cell)) {
                printStr += $"{neighbor.name},";
            }
            printStr += "],";
        }
        printStr += "}";
        Debug.Log(printStr);
    }
}
