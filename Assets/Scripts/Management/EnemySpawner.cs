using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    private static int _cellId = 0;

    public static EnemySpawner Instance;
    public GameObject EnemyPrefab;
    public int _numEnemiesToSpawn = 0;
    public int EnemySpawnIntervalSeconds = 1;

    private const float _minimumSpawnDistanceFromPlayer = 10f;
    private const float _minimumSpawnDistanceFromPlayerSqrd = _minimumSpawnDistanceFromPlayer * _minimumSpawnDistanceFromPlayer;

    void Awake() {
        Instance = this;
    }

    void SpawnEnemy() {
        var enemy = Instantiate(EnemyPrefab, GenerateSpawnPoint(), Quaternion.identity, EnemiesContainer.Instance.transform);
        enemy.name = "Cell_" + _cellId++;
        enemy.GetComponent<TeamTracker>().ChangeTeam(Team.Enemy);
        _numEnemiesToSpawn -= 1;
    }

    Vector3 GenerateSpawnPoint() {
        Vector3 spawnPoint;
        do {
            spawnPoint = new Vector3(
                UnityEngine.Random.Range(-Globals.ArenaWidth/2 + 1, Globals.ArenaWidth/2 - 1),
                0,
                UnityEngine.Random.Range(-Globals.ArenaHeight/2 + 1, Globals.ArenaHeight/2 - 2)
            ); }
        while (
            GameManager.Instance != null
            && GameManager.Instance.CurrentPlayer != null
            && (GameManager.Instance.CurrentPlayer.transform.position - spawnPoint).sqrMagnitude < _minimumSpawnDistanceFromPlayerSqrd
            );
        return spawnPoint;
    }

    public void SpawnMoreEnemies(int numEnemies) {
        _numEnemiesToSpawn += numEnemies;
        Invoke(nameof(SpawnEnemyOnInterval), EnemySpawnIntervalSeconds);
    }

    void SpawnEnemyOnInterval() {
        if (_numEnemiesToSpawn > 0) {
            SpawnEnemy();
            Invoke(nameof(SpawnEnemyOnInterval), EnemySpawnIntervalSeconds);
        }
    }
}