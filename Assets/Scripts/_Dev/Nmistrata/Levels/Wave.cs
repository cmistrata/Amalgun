using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave")]
public class Wave : ScriptableObject
{
    [SerializeField]
    private float _enemySpawnDelaySeconds;
    [SerializeField]
    private bool _randomizeOrder;
    [SerializeField]
    private List<CellType> _enemies;

    FixedDelayTimer _timer;

    public Wave()
    {
        _timer = new(_enemySpawnDelaySeconds);
    }

    public CellType GetNextEnemy()
    {
        int index = _randomizeOrder
            ? Random.Range(0, _enemies.Count)
            : _enemies.Count - 1;

        CellType enemy = _enemies[index];
        _enemies.RemoveAt(index);
        _timer.Reset(); //manual reset in case we bypass ShouldSpawn with a forced spawn
        return enemy;
    }

    public bool ShouldSpawn(float timePassed)
    {
        return !IsWaveComplete() && _timer.HasTimerTripped(timePassed);
    }

    public bool IsWaveComplete()
    {
        return !_enemies.Any();
    }
}
