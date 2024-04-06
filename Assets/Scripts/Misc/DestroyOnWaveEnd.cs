using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class DestroyOnWaveEnd : MonoBehaviour
{
    private bool destroyQueued = false;

    private void Awake() {
        EnemySpawnerAndCounter.SignalWaveOver += HandleWaveOver;
    }

    void HandleWaveOver() {
        if (!destroyQueued) {
            destroyQueued = true;
            Invoke("SelfDestruct", Random.Range(0f, 0.7f));
        }
    }

    void SelfDestruct() {
        Destroy(gameObject);
    }
}
