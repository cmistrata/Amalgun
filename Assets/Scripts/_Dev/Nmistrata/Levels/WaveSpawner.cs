using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour {
    public static event Action SignalWaveComplete;

    private WaveWrapper _wave;

    private bool _isWaveInProgress;
    private int _activeEnemies;
    private const float _minimumSpawnDistanceFromPlayerSquared = (10f * 10f);

    public WaveSpawner() {
        _wave = new WaveWrapper();
        _isWaveInProgress = false;
        _activeEnemies = 0;
    }

    public void LoadWave(Wave wave) {
        _wave.LoadWave(wave);
    }

    public void StartWave() {
        CellHealthManager.SignalEnemyCellDefeat += OnEnemyDeath;
        _isWaveInProgress = true;
    }

    public void Update() {
        if (_wave == null || !_isWaveInProgress) return;

        if (_wave.ShouldSpawn(Time.deltaTime)) {
            CellType cellType = _wave.GetNextEnemy();
            EnemySpawner.SpawnEnemy(cellType);
            _activeEnemies++;
        }
    }

    private void OnEnemyDeath() {
        _isWaveInProgress = --_activeEnemies > 0 || !_wave.IsWaveComplete();

        if (!_isWaveInProgress) {
            Debug.Log($"Wave Complete");
            CellHealthManager.SignalEnemyCellDefeat -= OnEnemyDeath;
            SignalWaveComplete.Invoke();
        }
    }
}
