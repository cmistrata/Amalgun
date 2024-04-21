using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WaveSpawner
{
    public static event Action SignalWaveComplete;

    private WaveWrapper _wave;

    private bool _isWaveInProgress;
    private int _activeEnemies;
    private const float _minimumSpawnDistanceFromPlayerSquared = (10f * 10f);

    public WaveSpawner()
    {
        _wave = new WaveWrapper();
        _isWaveInProgress = false;
        _activeEnemies = 0;
    }

    public void LoadWave(Wave wave)
    {
        Debug.Log($"Loading _wave {wave}");
        _wave.LoadWave(wave);
    }

    public void StartWave()
    {
        CellHealthManager.SignalEnemyCellDeath += OnEnemyDeath;
        _isWaveInProgress = true;
    }

    public void Update(float timePassed)
    {
        if (_wave == null || !_isWaveInProgress) return;

        if (_wave.ShouldSpawn(timePassed))
        {
            GameObject cell = CellPool.GetCell(_wave.GetNextEnemy());
            cell.transform.position = GenerateSpawnPoint();
            CellUtils.ConvertToTeam(cell, Team.Enemy);
            ++_activeEnemies;
        }
    }

    private Vector3 GenerateSpawnPoint()
    {
        Vector3 spawnPoint;
        do
        {
            spawnPoint = new Vector3(
                UnityEngine.Random.Range(-Globals.ArenaWidth / 2 + 1, Globals.ArenaWidth / 2 - 1),
                0,
                UnityEngine.Random.Range(-Globals.ArenaHeight / 2 + 1, Globals.ArenaHeight / 2 - 2)
            );
        }
        while (
            GameManager.Instance != null
            && GameManager.Instance.CurrentPlayer != null
            && (GameManager.Instance.CurrentPlayer.transform.position - spawnPoint).sqrMagnitude < _minimumSpawnDistanceFromPlayerSquared
            );
        return spawnPoint;
    }

    private void OnEnemyDeath()
    {
        _isWaveInProgress = (--_activeEnemies > 0 || !_wave.IsWaveComplete());

        if (!_isWaveInProgress)
        {
            Debug.Log($"Wave Complete");
            CellHealthManager.SignalEnemyCellDeath -= OnEnemyDeath;
            SignalWaveComplete.Invoke();
        }
    }
}