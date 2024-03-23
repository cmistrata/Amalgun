using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float TimeOutSeconds = 5f;
    protected float lifetime = 0;
    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= TimeOutSeconds)
        {
            Destroy(this.gameObject);
        }

        float x = transform.position.x;
        float y = transform.position.y;

        if (x > 32 || x < -32 || y > 20 || y < -20)
        {
            Destroy(gameObject);
        }

    }

    public void OnCollisionEnter2D(Collision2D other) {
        Destroy(this.gameObject);
    }

    public void OnCollisionEnter3D(Collision other) {
        Destroy(this.gameObject);
    }
}
