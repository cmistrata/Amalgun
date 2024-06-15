using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Amalgamator : MonoBehaviour {
    private const float _disconnectForceMagnitude = 500f;
    const float _timeToConnect = .03f;
    const float _connectionRadius = .53f;
    const float _baseRotationalInertia = 100f;
    const float _mergeTime = .6f;
    Dictionary<GameObject, HashSet<GameObject>> _cellGraph;
    Rigidbody _rb;
    private Dictionary<GameObject, float> _connectionTimeByCell = new();
    private HashSet<GameObject> _cellsBeingMerged = new();
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
        var cellType = cell.GetComponent<Cell>().Type;
        var touchingCells = FindTouchingCells(cell);
        // Try merging if possible.
        if (!_cellsBeingMerged.Contains(cell) && Globals.CellUpgradeByType.ContainsKey(cellType)) {
            foreach (var secondaryCell in touchingCells) {
                if (secondaryCell == gameObject || secondaryCell.GetComponent<Cell>().Type != cellType) continue;
                var tertiaryCells = GetLinkedCells(secondaryCell);
                tertiaryCells.UnionWith(touchingCells);
                tertiaryCells.Remove(secondaryCell);
                foreach (var tertiaryCell in tertiaryCells) {
                    if (tertiaryCell == gameObject || tertiaryCell.GetComponent<Cell>().Type != cellType) continue;
                    StartCoroutine(Merge(incomingCell: cell, secondaryCell: secondaryCell, tertiaryCell: tertiaryCell, cellType));
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
            if (cell1.TryGetComponent<Cell>(out var cellComponent)) {
                if (cellComponent.State != CellState.Player) {
                    cellComponent.ChangeState(CellState.Player);
                }
            }
            cell1.transform.parent = gameObject.transform;
        }
        if (!_cellGraph.ContainsKey(cell2)) {
            _cellGraph[cell2] = new();
            if (cell2.TryGetComponent<Cell>(out var cellComponent)) {
                if (cellComponent.State != CellState.Player) {
                    cellComponent.ChangeState(CellState.Player);
                }
            }
            cell2.transform.parent = gameObject.transform;
        }
        _cellGraph[cell1].Add(cell2);
        _cellGraph[cell2].Add(cell1);
    }

    void Disconnect(GameObject cell, bool neutralize) {
        RemoveCell(cell, neutralize);

        var disconnectedCells = GetUnconnectedCells();
        foreach (var disconnectedCell in disconnectedCells) {
            RemoveCell(disconnectedCell, neutralize);
        }
        UpdatePhysicalProperties();
    }

    void RemoveCell(GameObject cell, bool neutralize) {
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
        if (neutralize && gameObject.Cell()?.State != CellState.Neutral) {
            NeutralizeCell(cell);
        }
        _cellGraph.Remove(cell);
    }

    void NeutralizeCell(GameObject cell) {
        cell.Cell()?.ChangeState(CellState.Neutral);
        cell.transform.parent = Containers.Cells;

        // Propel the newly neutralized cell away from us.
        var cellRb = cell.Rigidbody();
        cellRb.linearVelocity = _rb.linearVelocity;
        Vector3 disconnectForce = (transform.position - gameObject.transform.position) * _disconnectForceMagnitude;
        cellRb.AddForce(disconnectForce);
    }

    HashSet<GameObject> GetLinkedCells(GameObject cell) {
        if (!_cellGraph.ContainsKey(cell)) {
            Debug.LogWarning($"Tried getting connected cells for cell {cell} not in graph.");
            return new HashSet<GameObject>();
        }
        return _cellGraph[cell];
    }

    IEnumerator Merge(GameObject incomingCell, GameObject secondaryCell, GameObject tertiaryCell, CellType cellType) {
        // Figure out which cell will become the merged cell.
        var secondaryCellDistance = (secondaryCell.transform.position - transform.position).sqrMagnitude;
        var tertiaryCellDistance = (tertiaryCell.transform.position - transform.position).sqrMagnitude;
        GameObject parentCell;
        GameObject childCell1 = incomingCell;
        GameObject childCell2;
        if (secondaryCellDistance < tertiaryCellDistance) {
            parentCell = secondaryCell;
            childCell2 = tertiaryCell;
        }
        else {
            parentCell = tertiaryCell;
            childCell2 = secondaryCell;
        }
        Debug.Log($"Parent cell: {parentCell}, childcell1: {childCell1}, childcell2: {childCell2}");


        // parentCell.Cell().ChangeState(CellState.Absorbing);
        _cellsBeingMerged.Add(parentCell);

        // Effectively disable the absorbee cells.
        childCell1.Cell().ChangeState(CellState.BeingAbsorbed);
        childCell1.transform.parent = Containers.Cells;
        Disconnect(childCell1, neutralize: false);

        childCell2.Cell().ChangeState(CellState.BeingAbsorbed);
        childCell2.transform.parent = Containers.Cells;
        Disconnect(childCell2, neutralize: false);

        // Move the absorbee cells into the absorber over time.
        float mergeTimer = 0f;
        float initialChildCell1Distance = Utils.DistanceBetween(childCell1, parentCell);
        float initialChildCell2Distance = Utils.DistanceBetween(childCell2, parentCell);
        while (mergeTimer < _mergeTime) {
            float proportionalDistance = (_mergeTime - Math.Abs(mergeTimer - .05f)) / (_mergeTime - .05f);
            float childCell1Distance = initialChildCell1Distance * proportionalDistance;
            childCell1.SetDistance(parentCell, childCell1Distance);
            float childCell2Distance = initialChildCell2Distance * proportionalDistance;
            childCell2.SetDistance(parentCell, childCell2Distance);
            mergeTimer += Time.deltaTime;
            yield return null;
        }

        // Replace the cells with the new merged cell.
        var newCellType = Globals.CellUpgradeByType[cellType];
        var newCellPrefab = Globals.CellPrefabByType[newCellType];
        Vector3 mergedCellPosition = parentCell.Position();
        Quaternion mergedCellRotation = parentCell.transform.rotation;
        GameObject newCell = Instantiate(newCellPrefab, position: mergedCellPosition, rotation: mergedCellRotation, parent: Containers.Cells);
        if (parentCell.TryGetComponent<Cannon>(out var parentCannon)) {
            newCell.GetComponent<Cannon>().CannonBase.transform.rotation = parentCannon.CannonBase.transform.rotation;
        }

        Disconnect(parentCell, neutralize: false);
        _cellsBeingMerged.Remove(parentCell);
        Destroy(parentCell);
        Destroy(childCell1);
        Destroy(childCell2);
        TryAmalgamate(newCell);
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
                Disconnect(secondaryCell, neutralize: true);
            }
        }
        else {
            Disconnect(cell, neutralize: false);
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
