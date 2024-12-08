using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class DelayedJoin {
    public readonly float ConnectionTime;
    public readonly float NearbyCellSqrDistance;
    public readonly GameObject HingeCell;

    public DelayedJoin(float connectionTime, float nearbyCellSqrDistance, GameObject hingeCell) {
        ConnectionTime = connectionTime;
        NearbyCellSqrDistance = nearbyCellSqrDistance;
        HingeCell = hingeCell;
    }
}

public class Amalgamator : MonoBehaviour {
    private const float _disconnectForceMagnitude = 45f;
    const float _timeToConnect = .3f;
    // How close two cells need to be to be joined together.
    const float _connectionRadius = .53f;
    // How close two cells need to be in order to be considered connection candidates
    // and delay immediate connection of a cell to allow a new one to slot into a better place.
    const float _nearbyRadius = 1.1f;
    const float _baseRotationalInertia = 100f;
    const float _mergeTime = .7f;
    const float _mergeGrowSize = 1.3f;
    const float _mergeGrowDuration = _mergeTime * .7f;
    const float _mergeShrinkSize = .8f;
    Dictionary<GameObject, HashSet<GameObject>> _cellGraph;
    Rigidbody _rb;
    private Dictionary<GameObject, DelayedJoin> _delayedJoinsByIncomingCell = new();
    // A list of cells currently being animated merged together.
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

    void Update() {
        HandleHinges();
    }


    /// <summary>
    /// Try to connect a cell, possibly merging it, and doing nothing if there aren't other cells nearby.
    /// </summary>
    void TryAmalgamate(GameObject cell) {
        var touchingCells = FindNearbyCells(cell, _connectionRadius);
        // Try merging if possible.
        if (TryMerging(cell, touchingCells)) return;
    }

    bool TryMerging(GameObject incomingCell, HashSet<GameObject> touchingCells) {
        var cellType = incomingCell.GetComponent<Cell>().Type;
        if (_cellsBeingMerged.Contains(incomingCell) || !Globals.CellUpgradeByType.ContainsKey(cellType)) return false;

        foreach (var secondaryCell in touchingCells) {
            if (secondaryCell == gameObject || secondaryCell.GetComponent<Cell>().Type != cellType) continue;
            var tertiaryCells = GetLinkedCells(secondaryCell);
            tertiaryCells.UnionWith(touchingCells);
            tertiaryCells.Remove(secondaryCell);
            foreach (var tertiaryCell in tertiaryCells) {
                if (tertiaryCell == gameObject || tertiaryCell.GetComponent<Cell>().Type != cellType) continue;
                StartCoroutine(Merge(incomingCell: incomingCell, secondaryCell: secondaryCell, tertiaryCell: tertiaryCell, cellType));
                CleanUpJoinData(incomingCell);
                return true;
            }
        }
        return false;
    }

    void ConnectToPlayer(GameObject incomingCell, IEnumerable<GameObject> touchingCells) {
        foreach (var touchingCell in touchingCells) {
            ConnectToCell(incomingCell, touchingCell);
        }
        CleanUpJoinData(incomingCell);
        UpdatePhysicalProperties();
    }

    /// <summary>
    /// Clean up data for an incoming cell that is now being joined, mainly connection timer and hinge joint if existing.
    /// </summary>
    /// <param name="incomingCell"></param>
    void CleanUpJoinData(GameObject incomingCell) {
        _delayedJoinsByIncomingCell.Remove(incomingCell);
    }

    void HandleHinges() {
        var cellsWithDestroyedHinges = new List<GameObject>();
        var cellsToConnect = new Dictionary<GameObject, HashSet<GameObject>>();
        foreach (var incomingCellAndDelayedJoin in _delayedJoinsByIncomingCell) {
            var incomingCell = incomingCellAndDelayedJoin.Key;
            var delayedJoin = incomingCellAndDelayedJoin.Value;
            if (delayedJoin.HingeCell == null) {
                cellsWithDestroyedHinges.Add(incomingCell);
                continue;
            }
            var touchingCells = FindNearbyCells(incomingCell, _connectionRadius);
            var nearbyCells = FindNearbyCells(incomingCell, _nearbyRadius);
            if (touchingCells.Count() != 0 &&
                (touchingCells.Count() >= 2
                || nearbyCells.Count() <= 1
                ||
                    nearbyCells.Where(cell => !touchingCells.Contains(cell))
                    .Select(cell => (cell.Position() - incomingCell.Position()).sqrMagnitude)
                    .Min() > (delayedJoin.NearbyCellSqrDistance + .03))) {
                cellsToConnect[incomingCell] = touchingCells;
                continue;
            }
            Utils.SetMinimumDistance(baseObject: delayedJoin.HingeCell, remoteObject: incomingCell, 1f);
        }

        foreach (var cell in cellsWithDestroyedHinges) {
            // If the hinge cell was destroyed before the incoming cell could attach, stop
            // the attachment of the incoming cell.
            CleanUpJoinData(cell);
            cell.GetComponent<Cell>().ChangeState(CellState.Neutral);
        }
        foreach (var cellAndTouching in cellsToConnect) {
            HandleIncomingCellConnectImmediately(cellAndTouching.Key, cellAndTouching.Value);
        }
    }


    void ConnectToCell(GameObject cell1, GameObject cell2) {
        if (!_cellGraph.ContainsKey(cell1)) {
            _cellGraph[cell1] = new();
            if (cell1.TryGetComponent<Cell>(out var cellComponent)) {
                if (cellComponent.State != CellState.Friendly) {
                    cellComponent.ChangeState(CellState.Friendly);
                }
            }
            cell1.transform.parent = gameObject.transform;
        }
        if (!_cellGraph.ContainsKey(cell2)) {
            _cellGraph[cell2] = new();
            if (cell2.TryGetComponent<Cell>(out var cellComponent)) {
                if (cellComponent.State != CellState.Friendly) {
                    cellComponent.ChangeState(CellState.Friendly);
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
            RemoveCell(disconnectedCell, neutralize: true);
        }
        UpdatePhysicalProperties();
    }

    void Disconnect(IEnumerable<GameObject> cells, bool neutralize) {
        foreach (var cell in cells) {
            RemoveCell(cell, neutralize);
        }

        var disconnectedCells = GetUnconnectedCells();
        foreach (var disconnectedCell in disconnectedCells) {
            RemoveCell(disconnectedCell, neutralize: true);
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
            if (_cellGraph.ContainsKey(neighbor)) {
                _cellGraph[neighbor].Remove(cell);
            }
            else {
                Debug.LogError($"During removal of {cell}, tried disconnecting cell-graph-untracked {neighbor}.");
            }
        }
        if (neutralize && cell.Cell()?.State != CellState.Neutral) {
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
        Vector3 disconnectForce = (cell.transform.position - transform.position).normalized * _disconnectForceMagnitude;
        cellRb.AddForce(disconnectForce, ForceMode.Impulse);
    }

    HashSet<GameObject> GetLinkedCells(GameObject cell) {
        if (!_cellGraph.ContainsKey(cell)) {
            Debug.LogWarning($"Tried getting connected cells for cell {cell} not in graph.");
            return new HashSet<GameObject>();
        }
        return _cellGraph[cell];
    }

    IEnumerator Merge(GameObject incomingCell, GameObject secondaryCell, GameObject tertiaryCell, CellType cellType) {
        AudioManager.Instance.PlayAbsorbSound();
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

        parentCell.Cell().ChangeState(CellState.Absorbing);
        _cellsBeingMerged.Add(parentCell);

        // Effectively disable the absorbee cells.
        childCell1.Cell().ChangeState(CellState.BeingAbsorbed);
        childCell1.transform.parent = Containers.Cells;
        childCell2.Cell().ChangeState(CellState.BeingAbsorbed);
        childCell2.transform.parent = Containers.Cells;
        Disconnect(new List<GameObject>() { childCell1, childCell2 }, neutralize: false);

        // Move the absorbee cells into the absorber over time.
        float mergeTimer = 0f;
        float initialChildCell1Distance = Utils.DistanceBetween(childCell1, parentCell);
        float initialChildCell2Distance = Utils.DistanceBetween(childCell2, parentCell);
        while (mergeTimer < _mergeTime) {
            // Do a scaling animation
            float scale = 1;
            if (mergeTimer < _mergeGrowDuration) {
                float growProportion = mergeTimer / _mergeGrowDuration;
                scale = Mathf.Lerp(1, _mergeGrowSize, growProportion);
            }
            else {
                float shrinkProportion = (mergeTimer - _mergeGrowDuration) / (_mergeTime - _mergeGrowDuration);
                scale = Mathf.Lerp(_mergeGrowSize, _mergeShrinkSize, shrinkProportion);
            }
            childCell1.SetScale(scale);
            childCell2.SetScale(scale);
            parentCell.SetScale(scale);

            // Move cells towards each other
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

        // Error: some neighbors don't exist when we try removing parent cell.
        Disconnect(parentCell, neutralize: false);
        Destroy(parentCell);
        _cellsBeingMerged.Remove(parentCell);
        _delayedJoinsByIncomingCell.Remove(parentCell);
        Destroy(childCell1);
        Destroy(childCell2);
        UpdatePhysicalProperties();
        HandleIncomingCellConnectImmediately(newCell);
        AudioManager.Instance.PlayMergedSound();
        EffectsManager.InstantiateEffect(Effect.Confetti, newCell.Position() + Vector3.up);
    }

    private HashSet<GameObject> FindNearbyCells(GameObject cell, float distance) {
        HashSet<GameObject> adjacentCells = new();

        Collider[] nearbyColliders = Physics.OverlapSphere(cell.transform.position, distance, Utils.GetLayerMask(Layers.PlayerCell));
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
            HandleIncomingCellFancy(collision.gameObject);
        }
    }

    /// <summary>
    /// Handle an incoming cell, either giving it time to gracefully connect if it can or connecting it immediately.
    /// </summary>
    /// <param name="incomingCell"></param>
    public void HandleIncomingCellFancy(GameObject incomingCell) {
        bool cellAlreadyConnectingOrMerging = _delayedJoinsByIncomingCell.ContainsKey(incomingCell) || _cellsBeingMerged.Contains(incomingCell);
        if (cellAlreadyConnectingOrMerging) return;

        AudioManager.Instance.PlayAttachSound();
        // If we can merge, connect and merge immediately. 
        var touchingCells = FindNearbyCells(incomingCell, _connectionRadius);
        if (TryMerging(incomingCell, touchingCells)) {
            return;
        }
        var nearbyCells = FindNearbyCells(incomingCell, _nearbyRadius);
        // Otherwise, give some time to slot the cell into place if its only touching one and there's a better nearby spot to be.
        if (touchingCells.Count() == 1 && nearbyCells.Count() > 1) {
            var nearestSqrDistance = nearbyCells
                .Where(cell => !touchingCells.Contains(cell))
                .Select(cell => (cell.Position() - incomingCell.Position()).sqrMagnitude)
                .Min();
            _delayedJoinsByIncomingCell[incomingCell] = new DelayedJoin(
                connectionTime: Time.time + _timeToConnect,
                nearbyCellSqrDistance: nearestSqrDistance,
                hingeCell: touchingCells.First()
            );
            incomingCell.GetComponent<Cell>().ChangeState(CellState.Attaching);
        }
        // Otherwise if the cell is already touching multiple, consider it in a good spot and connect it immediately.
        else {
            ConnectToPlayer(incomingCell, touchingCells);
        }
    }

    /// <summary>
    /// Handle an incoming cell by joining it immediately to the 
    /// </summary>
    /// <param name="incomingCell"></param>
    public void HandleIncomingCellConnectImmediately(GameObject incomingCell, HashSet<GameObject> touchingCells = null) {
        touchingCells ??= FindNearbyCells(incomingCell, _connectionRadius);
        if (touchingCells.Count() == 0) {
            Utils.LogOncePerSecond($"Tried connecting {incomingCell} but it was not touching any other cells.");
            return;
        }
        if (TryMerging(incomingCell, touchingCells)) {
            return;
        }
        else {
            ConnectToPlayer(incomingCell, touchingCells);
        }
    }

    public void OnCollisionStay(Collision collision) {
        if (enabled == false) return;
        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer != Layers.NeutralCell) return;

        if (Time.time >= _delayedJoinsByIncomingCell[collision.gameObject].ConnectionTime) {
            HandleIncomingCellConnectImmediately(collision.gameObject);
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

    public void OnMouseDown() {
        if (Utils.MouseRaycast(out RaycastHit hit)) {
            Debug.Log($"Clicked {hit.collider.gameObject}.");
            if (hit.collider.gameObject.TryGetComponent<Cell>(out var cell)) {
                if (cell.gameObject == this.gameObject) return;
                if (cell.State == CellState.Friendly) {
                    AudioManager.Instance.PlayAttachSound();
                    cell.ChangeState(CellState.Melded);
                }
            }
        }
    }
}
