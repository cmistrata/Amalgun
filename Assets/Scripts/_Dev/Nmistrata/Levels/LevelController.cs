using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class LevelController
{
    private Level _level;

    public static event Action SignalLevelComplete;

    private WaveSpawner _spawner;
    private int _currentWave;

    private int _waveSecondsDelay = 3;

    public LevelController()
    {
        _spawner = new WaveSpawner();
        Reset();
    }

    ~LevelController()
    {
    }
    
    public void LoadLevel(Level level)
    {
        _level = ScriptableObject.Instantiate(level);
        _currentWave = 0;
    }

    public void StartLevel()
    {
        WaveSpawner.SignalWaveComplete += OnWaveEnd;
        SpawnNextWave();
    }

    public void Update(float timePassed)
    {
        _spawner.Update(Time.deltaTime);
    }

    private void Reset()
    {
        _level = null;
    }

    private void OnWaveEnd()
    {
        if (_currentWave >= _level.Waves.Count)
        {
            SignalLevelComplete.Invoke();
            WaveSpawner.SignalWaveComplete -= OnWaveEnd;
            Reset();
            return;
        }
        GameManager.Instance.StartCoroutine(SpawnWaveRoutine(_waveSecondsDelay));
    }

    private IEnumerator SpawnWaveRoutine(int secondsDelay)
    {
        yield return new WaitForSeconds(secondsDelay);
        _spawner.LoadWave(_level.Waves[_currentWave++]);
        _spawner.StartWave();
    }
    private void SpawnNextWave()
    {
        _spawner.LoadWave(_level.Waves[_currentWave++]);
        _spawner.StartWave();
    }
}
