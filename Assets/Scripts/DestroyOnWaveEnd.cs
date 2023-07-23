using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Bullet))]
public class DestroyOnWaveEnd : MonoBehaviour
{
    private int startWave;
    private bool destroyQueued = false;
    private Bullet bullet;
    // Start is called before the first frame update
    void Start()
    {
        startWave = WavesManager.Instance.WaveIndex;
        bullet = GetComponent<Bullet>();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Make an event instead of checking all the tie
        if (WavesManager.Instance.WaveIndex > startWave && !destroyQueued && !bullet.isPlayerBullet) {
            destroyQueued = true;
            Invoke("SelfDestruct", Random.Range(0f, 0.7f));
        }
    }

    void SelfDestruct() {
        Destroy(gameObject);
    }
}
