using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(EnemySpawner))]
public class WavesManager : MonoBehaviour
{
    /// <summary>
    /// A list of configurations for each wave. For example, the first wave will be spawned using first WaveConfig in the list.
    /// </summary>
    public List<EnemySpawner.WaveConfig> Waves;

    public static WavesManager Instance;

    private EnemySpawner enemySpawner;
    public TMPro.TMP_Text WaveCounterText;
    public bool Spawning;
    public int EnemyCount = 0;

    public int WaveIndex = -1;

    public void Awake()
    {
        Instance = this;
        enemySpawner = gameObject.GetComponent<EnemySpawner>();
    }

    public void StartWave(int waveIndex)
    {

        WaveIndex = waveIndex;
        Debug.Log($"Spawning wave {waveIndex}");

        if (waveIndex != 0 && waveIndex % 3 == 0)
        {
            MusicManager.Instance.QueueSongStart(waveIndex / 3);
        }
        var waveConfig = Waves[Math.Min(waveIndex, Waves.Count - 1)];
        var enemies = enemySpawner.CreateEnemiesList(waveConfig);
        enemySpawner.SpawnEnemies(enemies, waveConfig.SpawnDuration);

        // Set the enemy count to the number of enemies queued to be spawned.
        // This will decrease when enemies are killed.
        EnemyCount += enemies.Count;
    }

    public void StartNextWave()
    {
        WaveIndex++;
        if (WaveIndex != 0)
        {
            AudioManager.Instance.PlayNewWaveSound();
        }
        WaveCounterText.text = $"Wave {WaveIndex + 1}";
        StartWave(WaveIndex);
    }

    public void EndWave()
    {
        StartNextWave();
    }
}
