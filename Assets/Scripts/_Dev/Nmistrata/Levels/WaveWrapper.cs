using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveWrapper
{
    private TimerBase _timer;
    private Wave _wave;
    private List<CellType> _remainingEnemies;

    public void LoadWave(Wave wave)
    {
        _timer = new FixedDelayTimer(wave.EnemySpawnDelaySeconds);
        _wave = wave;
        _remainingEnemies = new List<CellType>(_wave.Enemies);
    }

    public bool ShouldSpawn(float timePassed)
    {
        return !IsWaveComplete() && _timer.HasTimerTripped(timePassed);
    }

    public CellType GetNextEnemy()
    {
        if (_remainingEnemies?.Count == 0) return CellType.None;

        int index = _wave.RandomizeOrder
            ? Random.Range(0, _remainingEnemies.Count)
            : _remainingEnemies.Count - 1;

        CellType enemy = _remainingEnemies[index];
        _remainingEnemies.RemoveAt(index);
        _timer.Reset(); //manual reset in case we bypass ShouldSpawn with a forced spawn
        return enemy;
    }

    public bool IsWaveComplete() => !_remainingEnemies.Any();
}
