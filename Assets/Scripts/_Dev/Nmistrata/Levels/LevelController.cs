using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro.Examples;
using UnityEngine;

public class LevelController : MonoBehaviour {
    public static event Action SignalLevelComplete;

    public WaveSpawner Spawner;
    private ReadOnlyCollection<Wave> _waves;
    private int _currentWave;

    private int _waveSecondsDelay = 3;

    public LevelController() {
        _waves = null;
    }

    public void LoadLevel(Level level) {
        _waves = level.Waves;
        _currentWave = 0;
    }

    public void StartLevel() {
        WaveSpawner.SignalWaveComplete += OnWaveEnd;
        SpawnNextWave();
    }

    public void EndLevel() {
        _waves = null;
        WaveSpawner.SignalWaveComplete -= OnWaveEnd;
        SignalLevelComplete.Invoke();
    }

    private void OnWaveEnd() {
        if (_currentWave >= _waves.Count) {
            EndLevel();
            return;
        }
        StartCoroutine(SpawnWaveRoutine(_waveSecondsDelay));
    }

    private IEnumerator SpawnWaveRoutine(int secondsDelay) {
        yield return new WaitForSeconds(secondsDelay);
        SpawnNextWave();
    }

    private void SpawnNextWave() {
        Spawner.LoadWave(_waves[_currentWave++]);
        Spawner.StartWave();
    }
}
