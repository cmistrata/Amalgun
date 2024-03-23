using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovingBody2D))]
public class EnemyController : MonoBehaviour
{
    public bool Shooting = false;
    public float MovingRecalculateIntervalSeconds = 3f;
    private float _movingRecalculateTimer;
    private MovingBody2D _movingBody;

    private void Awake()
    {
        _movingBody = GetComponent<MovingBody2D>();
    }
    void Start()
    {
        _movingBody.TargetDirection = DetermineTargetDirection();
        _movingRecalculateTimer = MovingRecalculateIntervalSeconds;
    }
    void Update()
    {
        // Update moving timer
        _movingRecalculateTimer -= Time.deltaTime;
        if (_movingRecalculateTimer <= 0) {
            _movingBody.TargetDirection = DetermineTargetDirection();
            _movingRecalculateTimer = MovingRecalculateIntervalSeconds;
        }
    }

    public Vector2 DetermineTargetDirection() {
        // Move randomly
        float x = UnityEngine.Random.Range(-15, 15);
        float y = UnityEngine.Random.Range(-9, 9);
        Vector3 targetLocation = new Vector3(x, y, 0);

        return (targetLocation - transform.position).normalized;
    }
}
