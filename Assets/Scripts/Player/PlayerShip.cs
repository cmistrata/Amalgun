using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerShip
{

    private const float ATTACH_TOLERANCE = .1f;
    private const float DESTROY_BULLET_ON_CONNECT_DISTANCE = .5f;
    private const float BASE_ROTATIONAL_INERTIA = 100f;

    [SerializeField]
    private Dictionary<GameObject, List<GameObject>> _cellGraph = new();
    private GameObject _baseCell;

    public PlayerShip(GameObject initialCell)
    {
        _cellGraph.Add(initialCell, new());
        _baseCell = initialCell;
    }

    public float GetRotationalInertia()
    {
        float rotationalInertia = BASE_ROTATIONAL_INERTIA;
        foreach (GameObject cell in _cellGraph.Keys)
        {
            rotationalInertia += (cell.transform.position - _baseCell.transform.position).sqrMagnitude * 15f;
        }
        return rotationalInertia;
    }

    public void ConnectCell(GameObject cell)
    {
        if (!cell.TryGetComponent<CapsuleCollider>(out var collider))
        {
            Debug.LogError("Failed to attach cell due to missing capsule collider");
            return;
        }

        //prevent instantly being hit by bullets colliding with the attached cell
        DestroyNearbyBullets(cell.transform.position, collider.radius + DESTROY_BULLET_ON_CONNECT_DISTANCE);
        List<GameObject> nearbyCells = FindNearbyCells(cell.transform, collider.radius + ATTACH_TOLERANCE);

        if (!nearbyCells.Any())
        {
            Debug.LogError("Failed to find any nearby cells to attach to");
            return;
        }

        _cellGraph.Add(cell, nearbyCells);
        foreach (GameObject nearbyCell in nearbyCells)
        {
            _cellGraph[nearbyCell].Add(cell);
        }

        cell.transform.position = GetAttachPosition(cell.transform, nearbyCells);
    }

    //returns the list of all the resulting disconnected cells
    public List<GameObject> DisconnectCell(GameObject cell)
    {
        //remove all the connections involving this cell
        _cellGraph[cell].Clear();
        foreach (List<GameObject> connectedCells in _cellGraph.Values)
        {
            connectedCells.Remove(cell);
        }

        //determine which cells are now disconnected and remove them from the graph
        List<GameObject> disconnectedCells = GetDisonnectedCells();
        foreach (GameObject disconnectedCell in disconnectedCells)
        {
            _cellGraph.Remove(disconnectedCell);
        }

        return disconnectedCells;
    }

    private List<GameObject> GetDisonnectedCells()
    {
        return _cellGraph.Keys.Except(GetConnectedCells()).ToList();
    }

    private List<GameObject> GetConnectedCells()
    {
        List<GameObject> connectedCells = new(){ _baseCell };
        Stack<GameObject> cellsToCheck = new();
        cellsToCheck.Push(_baseCell);

        while(cellsToCheck.TryPop(out GameObject cellToCheck))
        {
            foreach (GameObject connectedCell in _cellGraph[cellToCheck])
            {
                if (!connectedCells.Contains(connectedCell))
                {
                    connectedCells.Add(connectedCell);
                    cellsToCheck.Push(connectedCell);
                }
            }
        }
        return connectedCells;
    }
    private void DestroyNearbyBullets(Vector3 position, float distance)
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(position, distance);
        foreach (Collider nearbyCollider in nearbyColliders)
        {
            GameObject nearbyObject = nearbyCollider.gameObject;
            if (nearbyObject.GetComponent<Bullet>() != null) UnityEngine.Object.Destroy(nearbyObject);
        }
    }

    private List<GameObject> FindNearbyCells(Transform transform, float distance)
    {
        List<GameObject> adjacentCells = new();

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, distance);
        foreach (Collider nearbyCollider in nearbyColliders)
        {
            GameObject nearbyObject = nearbyCollider.gameObject;
            if (transform != nearbyObject.transform && _cellGraph.ContainsKey(nearbyObject))
            {
                adjacentCells.Add(nearbyObject);
            }
        }

        return adjacentCells;
    }

    public Vector3 GetAttachPosition(GameObject cell)
    {
        if (!cell.TryGetComponent<CapsuleCollider>(out var collider))
        {
            Debug.LogError("Failed to calculate attach position due to missing capsule collider");
            return cell.transform.position;
        }

        return GetAttachPosition(cell.transform, FindNearbyCells(cell.transform, collider.radius + ATTACH_TOLERANCE));
    }

    //TODO: use this to 'snap' cells into position when they attach to multiple other cells
    private Vector3 GetAttachPosition(Transform cell, List<GameObject> attachmentCells)
    {
        return cell.position;
    }

    public override string ToString()
    {
        string printString = "";
        foreach (GameObject cell in _cellGraph.Keys)
        {
            printString += $"\n{cell.name}: {GameObjectListToString(_cellGraph[cell])}";
        }
        return printString;
    }

    //TODO move somewhere that makes more sense
    private string GameObjectListToString(List<GameObject> objects)
    {
        string printString = "[";
        string sep = "";
        foreach (GameObject obj in objects)
        {
            printString += sep + obj.name;
            sep = ", ";
        }
        printString += "]";
        return printString;
    }

}
