using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovingBody))]
public class EnemyController : MonoBehaviour
{
    public bool Shooting = false;
    public float MovingRecalculateIntervalSeconds = 3f;
    private float _movingRecalculateTimer;
    private MovingBody _movingBody;

    
    public virtual Vector3 GetShootingTarget() {
        return Vector3.up;
    }

    private void Awake()
    {
        _movingBody = GetComponent<MovingBody>();
    }
    void Start()
    {
        _movingBody.TargetDirection = DetermineTargetDirection();
        _movingRecalculateTimer = MovingRecalculateIntervalSeconds;
    }
    void Update()
    {
        // If veering off screen, automatically update
        if (Math.Abs(transform.position.x) > 15 || Math.Abs(transform.position.y) > 8) {
            _movingBody.TargetDirection = DetermineTargetDirection();
        }
        // Update moving timer
        _movingRecalculateTimer -= Time.deltaTime;
        if (_movingRecalculateTimer <= 0) {
            _movingBody.TargetDirection = DetermineTargetDirection();
            _movingRecalculateTimer = MovingRecalculateIntervalSeconds;
        }
    }

    /// <summary>
    /// Sent when an incoming collider makes contact with this object's
    /// collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    public void OnCollisionEnter2D(Collision2D other) 
    {
        // Get this enemy's part child
        Part part = GetComponentInChildren<Part>();
        if (part == null) {
            Debug.LogError($"Part was null for enemy {this} during collision detection.");
            return;
        }
        // Check if enemy has collided with a player part
        if (part.team == Team.Enemy) {
            Part playerPart = other.gameObject.GetComponent<Part>();
            if (playerPart == null) {
                return;
            }
            Rigidbody2D enemyRb2d = GetComponent<Rigidbody2D>();

            // Get the direction to rebound
            Vector2 reboundDir = (other.transform.position - transform.position).normalized * -1;
            enemyRb2d.velocity = reboundDir * 3;
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
