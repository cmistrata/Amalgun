using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StayInBounds : MonoBehaviour
{
    private Rigidbody _rb;
    public float ForceMagnitude = 8000f;
    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (transform.position.x < -Globals.ArenaWidth/2 + .4) {
            _rb.AddForce(Vector3.right * ForceMagnitude);
        } else if (transform.position.x > Globals.ArenaWidth / 2 - .4) {
            _rb.AddForce(Vector3.left * ForceMagnitude);
        }

        if (transform.position.z < -Globals.ArenaHeight / 2 + 1) {
            _rb.AddForce(Vector3.forward * ForceMagnitude);
        } else if (transform.position.z > Globals.ArenaHeight / 2) {
            _rb.AddForce(Vector3.back * ForceMagnitude);
        }
    }
}
