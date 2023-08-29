using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class DestroyOnWaveEnd : MonoBehaviour
{
    private int startWave;
    private bool destroyQueued = false;
    // Start is called before the first frame update
    void Start()
    {
        startWave = EnemySpawnerAndCounter.Instance.CurrentWave;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Make an event instead of checking all the tie
        if (EnemySpawnerAndCounter.Instance.CurrentWave > startWave && !destroyQueued && gameObject.layer != Layers.PlayerBullet) {
            destroyQueued = true;
            Invoke("SelfDestruct", Random.Range(0f, 0.7f));
        }
    }

    void SelfDestruct() {
        Destroy(gameObject);
    }
}
