using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    private static int _cellId = 0;

    public static EnemySpawner Instance;


    private const float _minimumSpawnDistanceFromPlayer = 10f;
    private const float _minimumSpawnDistanceFromPlayerSqrd = _minimumSpawnDistanceFromPlayer * _minimumSpawnDistanceFromPlayer;

    void Awake() {
        Instance = this;

    }

    public static GameObject SpawnEnemy(CellType cellType) {
        var enemy = Instantiate(
            Globals.CellPrefabByType[cellType],
            GenerateSpawnPoint(),
            Quaternion.identity,
            Containers.Cells
        );
        enemy.name = $"Cell_{cellType}_{_cellId++}";
        enemy.GetComponent<Cell>().ChangeState(CellState.Enemy);
        EffectsManager.InstantiateEffect(Effect.RedSmoke, enemy.transform.position);
        return enemy;
    }

    static Vector3 GenerateSpawnPoint() {
        Vector3 spawnPoint;
        do {
            spawnPoint = new Vector3(
                UnityEngine.Random.Range(-Globals.ArenaWidth / 2 + 1, Globals.ArenaWidth / 2 - 1),
                0,
                UnityEngine.Random.Range(-Globals.ArenaHeight / 2 + 1, Globals.ArenaHeight / 2 - 2)
            );
            spawnPoint = Quaternion.AngleAxis(45, Vector3.up) * spawnPoint;
        }
        while ((GameManager.GetPlayerPosition() - spawnPoint).sqrMagnitude < _minimumSpawnDistanceFromPlayerSqrd);
        return spawnPoint;
    }
}
