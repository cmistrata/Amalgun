using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemySpawner : MonoBehaviour {
    private static int _cellId = 0;

    public static EnemySpawner Instance;
    // public static event Action<GameObject> SignalCellSpawn;


    private const float _spawnDelay = 1.5f;
    private const float _minimumSpawnDistanceFromPlayer = 8f;
    private const float _minimumSpawnDistanceFromPlayerSqrd = _minimumSpawnDistanceFromPlayer * _minimumSpawnDistanceFromPlayer;

    private static int _cellsLayerMask = Utils.GetLayerMask(Layers.EnemyCell, Layers.NeutralCell, Layers.PlayerCell);
    void Awake() {
        Instance = this;

    }

    public static GameObject SpawnEnemy(CellType cellType) {
        // Generate enemy details.
        var spawnPoint = GenerateSpawnPoint();
        var enemy = Instantiate(
            Globals.CellPrefabByType[cellType],
            spawnPoint,
            Quaternion.identity,
            Containers.Cells
        );
        enemy.name = $"{ConvertToCellName(cellType)}_{_cellId++}";
        enemy.GetComponent<Cell>().ChangeState(CellState.Enemy);

        // Deactivate the enemy, spawning a spawn signal initially, and then
        // reactivating it to actually spawn it in.
        EffectsManager.InstantiateEffect(Effect.EnemySpawnCircle, spawnPoint, _spawnDelay);
        enemy.SetActive(false);
        Instance.StartCoroutine(ActivateEnemyAfterDelay(enemy, _spawnDelay));
        return enemy;
    }

    static IEnumerator<WaitForSeconds> ActivateEnemyAfterDelay(GameObject enemy, float delay) {
        yield return new WaitForSeconds(delay);
        if (enemy != null) {
            EffectsManager.InstantiateEffect(Effect.RedSmoke, enemy.transform.position);
            enemy.SetActive(true);
        }
    }

    private static string ConvertToCellName(CellType cellType) {
        var cellTypeStr = cellType.ToString();
        var cellTypeLastChar = cellTypeStr[^1];
        if (!char.IsNumber(cellTypeLastChar)) {
            return $"{cellTypeStr}Cell";
        }
        else {
            return $"{cellTypeStr[..^1]}Cell{cellTypeLastChar}";
        }
    }

    static Vector3 GenerateSpawnPoint() {
        Vector3 spawnPoint;
        do {
            float x, z;
            if (Utils.Chance(.5f)) {
                x = UnityEngine.Random.Range(Globals.ArenaWidth / 2 - 5, Globals.ArenaWidth / 2 - 1);
                z = UnityEngine.Random.Range(0, Globals.ArenaHeight / 2 - 1);
            }
            else {
                x = UnityEngine.Random.Range(0, Globals.ArenaWidth / 2 - 1);
                z = UnityEngine.Random.Range(Globals.ArenaHeight / 2 - 5, Globals.ArenaHeight / 2 - 1);
            }
            if (Utils.Chance(.5f)) x *= -1;
            if (Utils.Chance(.5f)) z *= -1;
            spawnPoint = new Vector3(x, 0, z);
            spawnPoint = Quaternion.AngleAxis(45, Vector3.up) * spawnPoint;
        }
        while ((GameManager.GetPlayerPosition() - spawnPoint).sqrMagnitude < _minimumSpawnDistanceFromPlayerSqrd && Utils.AnyNearbyGameObjects(spawnPoint, distance: .5f, layerMask: _cellsLayerMask));
        return spawnPoint;
    }
}
