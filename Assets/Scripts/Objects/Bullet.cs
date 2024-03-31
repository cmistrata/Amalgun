using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float TimeOutSeconds = 5f;
    protected float lifetime = 0;
    private Rigidbody _rb;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        lifetime += Time.deltaTime;

        float x = transform.position.x;
        float y = transform.position.y;

        //if (x > 32 || x < -32 || y > 20 || y < -20)
        //{
        //    Destroy(gameObject);
        //}

    }

    private void FixedUpdate() {
        if (lifetime >= TimeOutSeconds && !_rb.useGravity) {
            _rb.useGravity = true;
        }
    }

    public void OnCollisionEnter2D(Collision2D other) {
        Destroy(this.gameObject);
    }

    public void OnCollisionEnter(Collision other) {
        Destroy(this.gameObject);
    }
}
