using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerShip {
    private const float ATTACH_TOLERANCE = .2f;
    private const float DESTROY_BULLET_ON_CONNECT_DISTANCE = .5f;

    [SerializeField]
    private Dictionary<GameObject, List<GameObject>> _cellGraph = new();
    private GameObject _baseCell;

    public PlayerShip(GameObject initialCell) {
        _cellGraph.Add(initialCell, new());
        _baseCell = initialCell;
    }

    public void ConnectCell(GameObject cell) {
        if (!cell.TryGetComponent<CapsuleCollider>(out var collider)) {
            Debug.LogError("Failed to attach cell due to missing capsule collider");
            return;
        }

        //prevent instantly being hit by bullets colliding with the attached cell
        // DestroyNearbyBullets(cell.transform.position, collider.radius + DESTROY_BULLET_ON_CONNECT_DISTANCE);
        List<GameObject> nearbyCells = FindNearbyCells(cell.transform, collider.radius + ATTACH_TOLERANCE);

        if (!nearbyCells.Any()) {
            Debug.LogError("Failed to find any nearby cells to attach to");
            return;
        }

        _cellGraph.Add(cell, nearbyCells);
        foreach (GameObject nearbyCell in nearbyCells) {
            _cellGraph[nearbyCell].Add(cell);
        }

        // cell.transform.position = GetAttachPosition(cell.transform, nearbyCells);
    }

    //remove the cell, and all the connections involving this cell
    public void RemoveCell(GameObject cell) {
        _cellGraph.Remove(cell);
        foreach (List<GameObject> connectedCells in _cellGraph.Values) {
            connectedCells.Remove(cell);
        }
    }

    public List<GameObject> GetAndRemoveDisconnectedCells() {
        var disconnectedCells = _cellGraph.Keys.Except(GetConnectedCells()).ToList();
        foreach (var cell in disconnectedCells) {
            RemoveCell(cell);
        }
        return disconnectedCells;
    }

    private List<GameObject> GetConnectedCells() {
        List<GameObject> connectedCells = new() { _baseCell };
        Stack<GameObject> cellsToCheck = new();
        cellsToCheck.Push(_baseCell);

        while (cellsToCheck.TryPop(out GameObject cellToCheck)) {
            foreach (GameObject connectedCell in _cellGraph[cellToCheck]) {
                if (!connectedCells.Contains(connectedCell)) {
                    connectedCells.Add(connectedCell);
                    cellsToCheck.Push(connectedCell);
                }
            }
        }
        return connectedCells;
    }
    private void DestroyNearbyBullets(Vector3 position, float distance) {
        Collider[] nearbyColliders = Physics.OverlapSphere(position, distance, Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (Collider nearbyCollider in nearbyColliders) {
            GameObject nearbyObject = nearbyCollider.gameObject;
            if (nearbyObject.GetComponent<Bullet>() != null) UnityEngine.Object.Destroy(nearbyObject);
        }
    }

    private List<GameObject> FindNearbyCells(Transform transform, float distance) {
        List<GameObject> adjacentCells = new();

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, distance);
        foreach (Collider nearbyCollider in nearbyColliders) {
            GameObject nearbyObject = nearbyCollider.gameObject;
            if (transform != nearbyObject.transform
                    && nearbyCollider.isTrigger == false
                    && _cellGraph.ContainsKey(nearbyObject)) {
                adjacentCells.Add(nearbyObject);
            }
        }

        return adjacentCells;
    }

    public Vector3 GetAttachPosition(GameObject cell) {
        if (!cell.TryGetComponent<CapsuleCollider>(out var collider)) {
            Debug.LogError("Failed to calculate attach position due to missing capsule collider");
            return cell.transform.position;
        }

        return GetAttachPosition(cell.transform, FindNearbyCells(cell.transform, collider.radius + ATTACH_TOLERANCE));
    }

    //TODO: use this to 'snap' cells into position when they attach to multiple other cells
    private Vector3 GetAttachPosition(Transform cell, List<GameObject> attachmentCells) {
        while (attachmentCells.Count > 2) {
            attachmentCells.RemoveAt(0); //TODO: handle >2 connections more gracefully? Its probably pretty rare and this will be good enough
        }
        if (attachmentCells.Count == 1) {
            Transform otherCell = attachmentCells[0].transform;
            Vector3 direction = (cell.position - otherCell.position).normalized;
            float distance = otherCell.GetComponent<CapsuleCollider>().radius + cell.GetComponent<CapsuleCollider>().radius;
            return otherCell.position + (direction * distance);
        }
        if (attachmentCells.Count == 2) {
            float myRadius = cell.GetComponent<CapsuleCollider>().radius;
            float distance1 = attachmentCells[0].GetComponent<CapsuleCollider>().radius + myRadius;
            float distance2 = attachmentCells[1].GetComponent<CapsuleCollider>().radius + myRadius;
            Vector2 center1 = new(attachmentCells[0].transform.position.x, attachmentCells[0].transform.position.z);
            Vector2 center2 = new(attachmentCells[1].transform.position.x, attachmentCells[1].transform.position.z);

            float distanceBetweenConnections = Vector2.Distance(center1, center2);

            float a = (distance1 * distance1 - distance2 * distance2 + distanceBetweenConnections * distanceBetweenConnections) / (2 * distanceBetweenConnections);
            float h = Mathf.Sqrt(distance1 * distance2 - a * a);

            Vector2 direction = (center2 - center1).normalized;

            Vector2 intersection1 = center1 + a * direction + h * new Vector2(-direction.y, direction.x);
            Vector2 intersection2 = center1 + a * direction - h * new Vector2(-direction.y, direction.x);

            Vector3 position1 = new Vector3(intersection1.x, cell.transform.position.y, intersection1.y);
            Vector3 position2 = new Vector3(intersection2.x, cell.transform.position.y, intersection2.y);

            return Vector3.Distance(position1, cell.position) < Vector3.Distance(position2, cell.position)
                ? position1
                : position2;
        }
        return cell.position;
    }


    public override string ToString() {
        string printString = "";
        foreach (GameObject cell in _cellGraph.Keys) {
            printString += $"\n{cell.name}: {GameObjectListToString(_cellGraph[cell])}";
        }
        return printString;
    }

    //TODO move somewhere that makes more sense
    private string GameObjectListToString(List<GameObject> objects) {
        string printString = "[";
        string sep = "";
        foreach (GameObject obj in objects) {
            printString += sep + obj.name;
            sep = ", ";
        }
        printString += "]";
        return printString;
    }

}
