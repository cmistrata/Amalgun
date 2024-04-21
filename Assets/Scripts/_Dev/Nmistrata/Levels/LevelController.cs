using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro.Examples;
using UnityEngine;

public class LevelController
{
    public static event Action SignalLevelComplete;

    private WaveSpawner _spawner;
    private ReadOnlyCollection<Wave> _waves;
    private int _currentWave;

    private int _waveSecondsDelay = 3;

    public LevelController()
    {
        _spawner = new WaveSpawner();
        _waves = null;
    }
    
    public void LoadLevel(Level level)
    {
        _waves = level.Waves;
        _currentWave = 0;
    }

    public void StartLevel()
    {
        WaveSpawner.SignalWaveComplete += OnWaveEnd;
        SpawnNextWave();
    }
    public void EndLevel()
    {
        _waves = null;
        WaveSpawner.SignalWaveComplete -= OnWaveEnd;
        SignalLevelComplete.Invoke();
    }

    public void Update(float timePassed)
    {
        _spawner.Update(Time.deltaTime);
    }

    private void OnWaveEnd()
    {
        if (_currentWave >= _waves.Count)
        {
            EndLevel();
            return;
        }
        GameManager.Instance.StartCoroutine(SpawnWaveRoutine(_waveSecondsDelay));
    }

    private IEnumerator SpawnWaveRoutine(int secondsDelay)
    {
        yield return new WaitForSeconds(secondsDelay);
        SpawnNextWave();
    }
    private void SpawnNextWave()
    {
        _spawner.LoadWave(_waves[_currentWave++]);
        _spawner.StartWave();
    }
}
