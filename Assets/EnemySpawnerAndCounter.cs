using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerAndCounter : MonoBehaviour
{
    public GameObject enemyPrefab;

    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemy();
    }

    void SpawnEnemy() {
        var enemy = Instantiate(enemyPrefab, new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity);
        enemy.GetComponent<Part>().SignalEnemyDeath += SpawnEnemy;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
