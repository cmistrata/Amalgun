using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class EnemySpawner : MonoBehaviour
{
    private List<(GameObject, float)> SpawnQueue = new List<(GameObject, float)>();
    private float waveTimer = 0f;
    private int enemyNumber = 1;
    private System.Random random;

    /// <summary>
    /// The game object to spawn enemies under.
    /// </summary>
    public Transform EnemyParent;
    public List<EnemyTuple> DifficultyMapping;
    public float ArenaHeight = 20f;
    public float ArenaWidth = 40f;


    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>

    void Start()
    {
        random = new System.Random();
    }
    void Update()
    {
        waveTimer += Time.deltaTime;
        while (SpawnQueue.Count > 0 && waveTimer > SpawnQueue[0].Item2)
        {
            SpawnEnemy(SpawnQueue[0].Item1);
            SpawnQueue.RemoveAt(0);
            if (SpawnQueue.Count == 0)
            {
                // The wave has finished spawning
                WavesManager.Instance.Spawning = false;
            }
        }
    }

    /// <summary>
    /// Samples a guassian curve
    /// </summary>
    /// <param name="mean"></param>
    /// <param name="std"></param>
    /// <returns></returns>
    private float SampleGuassian(float mean, float std)
    {
        double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                    Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal = mean + std * randStdNormal; //random normal(mean,stdDev^2)
        return (float)randNormal;
    }

    /// <summary>
    /// Creates a Wave Dictionary based on the paramaters given. Uses a gaussian distribution centered on difficultyBias with std standard deviation
    /// </summary>
    /// <param name="waveConfig">Configuration used to determine how many and what difficulty of enemies to spawn.</param>
    /// <param name="difficultyMapping">Optional param to set a custom difficulty mapping</param>
    /// <returns>A wave dictionary that can be passed to spawnWave</returns>
    public List<GameObject> CreateEnemiesList(WaveConfig waveConfig, List<EnemyTuple> difficultyMapping = null)
    {
        // Option to set your own difficulty mapping
        difficultyMapping = difficultyMapping ?? this.DifficultyMapping; ;
        int runningDifficultyTotal = 0;
        List<GameObject> enemies = new List<GameObject>();

        // Get the probabilities that each enemy should be spawned. This stores them in the difficulty mapping
        mapProbabilityDistribution(waveConfig.DifficultyBias, difficultyMapping);

        // Generate enemies using the probabilities, as well as the difficulty min and max.
        while (runningDifficultyTotal < waveConfig.DifficultyTotal)
        {
            // Generate a float from 0 to 1
            float randFloat = UnityEngine.Random.Range(0f, 1f);

            // Iterate through mappinds to get an enemy using the random
            int idx = 0;
            while (randFloat > difficultyMapping[idx].Probability && idx < difficultyMapping.Count - 1)
            {
                idx++;
                randFloat -= difficultyMapping[idx].Probability;
            }
            // If difficulty is out of range, redo.
            if (difficultyMapping[idx].Difficulty < waveConfig.DifficultyMin ||
                difficultyMapping[idx].Difficulty > waveConfig.DifficultyMax)
            {
                continue;
            }

            // If everything checks out, add its difficulty to the total
            runningDifficultyTotal += difficultyMapping[idx].Difficulty;
            enemies.Add(difficultyMapping[idx].Enemy);
        }
        return enemies;
    }
    /// <summary>
    /// Takes enemy tuples and gives them probabilities.
    /// </summary>
    /// <param name="difficultyBias"></param>
    /// <param name="difficultyMapping"></param>
    private void mapProbabilityDistribution(int difficultyBias, List<EnemyTuple> difficultyMapping = null)
    {
        if (difficultyMapping == null)
        {
            difficultyMapping = this.DifficultyMapping;
        }
        // The higher the difficulty difference, the lower the probability of spawning is. Minimum 1.
        // The probability that a difficulty difference of y should spawn is 1/yx, where x is calculated here:
        float x = 0f;
        foreach (EnemyTuple tup in difficultyMapping)
        {
            float difficultyDif = Math.Max(4f, Math.Abs(difficultyBias - tup.Difficulty));
            x += (1f / difficultyDif);
        }
        // Use this x to calculate difficulties
        foreach (EnemyTuple tup in difficultyMapping)
        {
            float difficultyDif = Math.Max(4f, Math.Abs(difficultyBias - tup.Difficulty));
            tup.Probability = 1f / (x * difficultyDif);
        }
    }

    /// <summary>
    /// Spawns a wave of enemies over a given period of time
    /// </summary>
    /// <param name="enemies">A list of enemies to spawn.</param>
    /// <param name="timeToSpawnAllEnemies">The total period of time over which the enemies should be spawned.</param>
    public void SpawnEnemies(List<GameObject> enemies, float timeToSpawnAllEnemies)
    {
        enemies.Shuffle();

        // Create a list of the times that enemies should spawn
        List<float> spawnTimes = new List<float>();
        float timePerEnemy = enemies.Count > 1 ? timeToSpawnAllEnemies / (enemies.Count - 1) : 0;
        for (float t = 0; t < enemies.Count; t++)
        {
            // Randomly change the spawn time so its not uniform
            float baseTime = timePerEnemy * t;
            float timeOffset = UnityEngine.Random.Range(-.2f * timeToSpawnAllEnemies, .2f * timeToSpawnAllEnemies);
            float newTime = System.Math.Max(0f, baseTime + timeOffset);
            spawnTimes.Add(newTime);
        }

        spawnTimes.Sort();

        // Create tuples of enemies and when they should spawn
        SpawnQueue = enemies.Zip(spawnTimes, (enemy, time) => (enemy, time)).ToList();
        waveTimer = 0f;
    }

    /// <summary>
    /// Spawns an enemy randomly around the screen
    /// /// </summary>
    public void SpawnEnemy(GameObject enemyPrefab)
    {
        if (Player2D.Instance == null) return;
        Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-(ArenaWidth / 2 - 1), (ArenaWidth / 2 - 1)), -(ArenaHeight / 2 - 1), (ArenaHeight / 2 - 1));
        // TODO: Clamp the spawn position away from the player instead of retrying over and over
        while (Vector3.Magnitude(spawnPosition - Player2D.Instance.transform.position) < 4f)
        {
            spawnPosition = new Vector3(UnityEngine.Random.Range(-(ArenaWidth / 2 - 1), (ArenaWidth / 2 - 1)), -(ArenaHeight / 2 - 1), (ArenaHeight / 2 - 1));
        }
        var enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.AngleAxis(0, Vector3.forward), parent: EnemyParent);
        enemy.name += $" {enemyNumber++}";
    }

    [System.Serializable]
    /// <summary>
    /// The configuration for a wave.
    /// </summary>
    public class WaveConfig
    {
        /// <summary>
        /// Difficulty rating to center distribution of enemies on.
        /// </summary>
        public int DifficultyBias;
        /// <summary>
        /// Max difficulty of enemy to spawn.
        /// </summary>
        public int DifficultyMax;
        /// <summary>
        /// Minimum difficulty of enemy to spawn.
        /// </summary>
        public int DifficultyMin;
        /// <summary>
        /// The total difficulty of enemies spawned in the wave.
        /// </summary>
        public int DifficultyTotal;
        /// <summary>
        /// How long it takes to spawn all the enemies.
        /// </summary>
        public float SpawnDuration;

        public WaveConfig(int difficultyBias = 1, int difficultyMax = 1000, int difficultyMin = 0, int difficultyTotal = 10, float spawnDuration = 5)
        {
            DifficultyBias = difficultyBias;
            DifficultyMax = difficultyMax;
            DifficultyMin = difficultyMin;
            DifficultyTotal = difficultyTotal;
            SpawnDuration = spawnDuration;
        }
    }

    [System.Serializable]
    public class EnemyTuple
    {
        public GameObject Enemy;
        public int Difficulty;
        public float Probability;

        public EnemyTuple(GameObject enemy, int difficulty, float probability = 0f)
        {
            Enemy = enemy;
            Difficulty = difficulty;
            Probability = probability;
        }
    }
}
