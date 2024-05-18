using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Pool;

public static class CellPool {
    [SerializeField] //just to verify theyre correctly loaded
    private static Dictionary<CellType, GameObject> _cellPrefabs;
    private static Dictionary<CellType, IObjectPool<GameObject>> _cellPools;
    private static int id = 0;

    static CellPool() {
        _cellPrefabs = new();
        _cellPools = new();

        var prefabs = Resources.LoadAll<GameObject>("Prefabs/Cells");
        foreach (GameObject prefab in prefabs) {
            if (!prefab.TryGetComponent<CellProperties>(out CellProperties cellType)) {
                Debug.LogError($"Failed to find cell properties component on cell prefab {prefab.name}");
                continue;
            }

            if (_cellPrefabs.ContainsKey(cellType.Type)) {
                Debug.LogError($"Loaded duplicate cell prafab for {DebugString.EnumToString(cellType.Type)}");
                continue;
            }

            Debug.Log($"Loaded cell prefab for {DebugString.EnumToString(cellType.Type)}");

            _cellPrefabs[cellType.Type] = prefab;
            _cellPools[cellType.Type] = new ObjectPool<GameObject>(
                () => CreateCell(cellType.Type),
                (cell) => InitializeCell(cell),
                (cell) => ReleaseCell(cell),
                (cell) => DestroyCell(cell),
                /*collection_check=*/true,
                /*default_capacity=*/10,
                /*max_size=*/30);
        }
    }

    private static GameObject CreateCell(CellType type) {
        return Object.Instantiate(_cellPrefabs[type]);
    }

    private static void InitializeCell(GameObject cell) {
        cell.SetActive(true);
        CellUtils.EnableMovement(cell);
    }
    private static void ReleaseCell(GameObject cell) {
        cell.SetActive(false);
    }
    private static void DestroyCell(GameObject cell) {
        Object.Destroy(cell);
    }

    public static GameObject GetCell(CellType type) {
        GameObject cell = _cellPools[type].Get();
        cell.name = "Cell" + id++;

        return cell;
    }

    public static GameObject GetCellPrefab(CellType type) {
        return _cellPrefabs[type];
    }

    public static void ReturnCell(GameObject cell, CellType type) {
        _cellPools[type].Release(cell);
    }

    public static void ReturnCell(GameObject cell) => ReturnCell(cell, CellUtils.GetCellType(cell));
}
