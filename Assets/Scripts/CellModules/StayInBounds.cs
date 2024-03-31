using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StayInBounds : MonoBehaviour
{
    private Rigidbody _rb;
    private float _forceMagnitude = 8000f;
    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.position.x < -Globals.ArenaWidth/2 + .4) {
            _rb.AddForce(Vector3.right * _forceMagnitude);
        } else if (transform.position.x > Globals.ArenaWidth / 2 - .4) {
            _rb.AddForce(Vector3.left * _forceMagnitude);
        }

        if (transform.position.z < -Globals.ArenaHeight / 2 + 1) {
            _rb.AddForce(Vector3.forward * _forceMagnitude);
        } else if (transform.position.z > Globals.ArenaHeight / 2) {
            _rb.AddForce(Vector3.back * _forceMagnitude);
        }
    }
}
