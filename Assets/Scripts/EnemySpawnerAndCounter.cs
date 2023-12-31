using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySpawnerAndCounter : MonoBehaviour
{
    public static EnemySpawnerAndCounter Instance;
    public GameObject enemyPrefab;
    public TMP_Text _waveText;
    public int CurrentWave = 1;
    public int EnemiesPerWave = 5;
    public int EnemiesLeftToKill;
    private int _enemiesSpawned;
    public int EnemySpawnIntervalSeconds = 1;

    void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _waveText.text = $"Wave {CurrentWave}";
        EnemiesLeftToKill = EnemiesPerWave;
        _enemiesSpawned = 0;
        SpawnEnemyOnInterval();
    }

    void SpawnEnemy() {
        var enemy = Instantiate(enemyPrefab, new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity, EnemiesContainer.Instance.transform);
        enemy.GetComponent<Part>().SignalEnemyDeath += DecrementEnemies;
        _enemiesSpawned += 1;
    }

    void DecrementEnemies() {
        EnemiesLeftToKill -= 1;
        if (EnemiesLeftToKill == 0) {
            StartNewWave();
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
