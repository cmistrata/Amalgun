using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField]
    private List<Wave> _waves;

    public static event Action SignalLevelComplete;

    private WaveSpawner _spawner;
    private int _currentWave;

    private int _waveSecondsDelay = 3;

    private void Start()
    {
        _spawner = new();
        WaveSpawner.SignalWaveComplete += SpawnNextWave;
        GameManager.SignalGameStart += SpawnNextWave;
        Reset();
    }

    private void Update()
    {
        _spawner.Update(Time.deltaTime);
    }

    private void Reset()
    {
        _currentWave = 0;
    }

    private void SpawnNextWave()
    {
        if (_currentWave >= _waves.Count)
        {
            Debug.Log("Ending Level");
            SignalLevelComplete.Invoke();
            Reset();
            return;
        }
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        yield return new WaitForSeconds(_waveSecondsDelay);
        _spawner.LoadWave(_waves[_currentWave++]);
        _spawner.StartWave();
    }
}
