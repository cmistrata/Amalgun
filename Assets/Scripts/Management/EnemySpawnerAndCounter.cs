using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySpawnerAndCounter : MonoBehaviour {
    public static EnemySpawnerAndCounter Instance;
    public GameObject enemyPrefab;
    public TMP_Text _waveText;
    public int CurrentWave = 1;
    public int EnemiesPerWave = 5;
    public int EnemiesLeftToKill;
    private int _enemiesSpawned;
    public int EnemySpawnIntervalSeconds = 1;

    private const float _minimumSpawnDistanceSqr = 100f;

    public static event Action SignalWaveOver;

    void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        _waveText.text = $"Wave {CurrentWave}";
        EnemiesLeftToKill = EnemiesPerWave;
        _enemiesSpawned = 0;
        SpawnEnemyOnInterval();
    }

    void SpawnEnemy() {
        var enemy = Instantiate(enemyPrefab, GenerateSpawnPoint(), Quaternion.identity, EnemiesContainer.Instance.transform);
        enemy.GetComponent<TeamTracker>().ChangeTeam(Team.Enemy);
        enemy.GetComponent<Part>().SignalEnemyDeath += DecrementEnemies;
        _enemiesSpawned += 1;
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
            && GameManager.Instance.Player != null
            && (GameManager.Instance.Player.transform.position - spawnPoint).sqrMagnitude < _minimumSpawnDistanceSqr
            );
        return spawnPoint;
    }

    void DecrementEnemies() {
        EnemiesLeftToKill -= 1;
        if (EnemiesLeftToKill == 0) {
            SignalWaveOver?.Invoke();
        }
    }

    void StartNewWave() {
        CurrentWave += 1;
        _waveText.text = $"Wave {CurrentWave}";
        EnemiesLeftToKill = EnemiesPerWave;
        _enemiesSpawned = 0;
        SpawnEnemyOnInterval();
    }

    void SpawnEnemyOnInterval() {
        if (EnemiesPerWave > _enemiesSpawned) {
            SpawnEnemy();
            Invoke("SpawnEnemyOnInterval", EnemySpawnIntervalSeconds);
        }
    }
}