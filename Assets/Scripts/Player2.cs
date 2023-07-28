using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2 : MonoBehaviour {
    private Rigidbody2D _rb;
    private MovingBody _movingBody;
    private Cannon _cannon;

    public float RotationSpeed = 100f;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _movingBody = GetComponent<MovingBody>();
        _cannon = GetComponent<Cannon>();
    }

    // Update is called once per frame
    void Update() {
        CheckInputs();
    }

    void CheckInputs() {
        // Movement
        Vector2 newTargetDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        _movingBody.TargetDirection = newTargetDirection.normalized;
        float clockwiseRotation = Input.GetAxis("Rotate Clockwise");
        transform.Rotate(new Vector3(0, 0, -clockwiseRotation * RotationSpeed * Time.deltaTime));
    }
}
