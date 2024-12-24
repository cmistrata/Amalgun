using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour {
    private Rigidbody _rb;
    private Mover _mover;
    private Amalgamator _amalgamator;
    public static Player Instance;
    public int Health = 4;

    public int MaxHealth = 4;

    private const float _torque = 3000f;
    private const float _newCellMassIncrease = 30f;

    public static event Action SignalPlayerDeath;

    public int MaxDashes = 3;
    public float CurrentDashCharge;
    private float _secondsPerDash = 9;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _mover = GetComponent<Mover>();
        _amalgamator = GetComponent<Amalgamator>();
        CurrentDashCharge = MaxDashes;

        Instance = this;
    }

    public void Update() {
        if (CurrentDashCharge < MaxDashes) {
            CurrentDashCharge += Time.deltaTime / _secondsPerDash;
        }
        if (CurrentDashCharge >= 1 && Input.GetKeyDown(KeyCode.Space)) {
            Vector3 newTargetDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            _mover.TargetDirection = newTargetDirection.normalized;
            _mover.Dash();
            CurrentDashCharge -= 1;
        }
    }

    private void FixedUpdate() {
        Vector3 newTargetDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _mover.TargetDirection = newTargetDirection.normalized;
        float clockwiseRotationInput = Input.GetAxis("Rotate Clockwise");
        _rb.AddTorque(_torque * clockwiseRotationInput * Vector3.up);
    }

    public void Heal(int amount) {
        //CurrentHealth = Math.Min(CurrentHealth + amount, MaxHealth);
    }

    private void ModifyCellToFollowShip(GameObject cell) {
        cell.transform.parent = transform;
        var rb = cell.GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Die() {
        PlayDeathFX();
        gameObject.SetActive(false);
        _amalgamator.HandlePlayerDeath();
        SignalPlayerDeath?.Invoke();
    }

    public void PlayDeathFX() {
        AudioManager.Instance.PlayCellDestroy();
        CinemachineCameraManager.Instance.Shake(1, .6f);
    }

    void OnCollisionEnter(Collision collision) {
        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer != Layers.EnemyBullet) return;

        var nonBulletObject = collision.GetContact(0).thisCollider.gameObject;
        var collisionIsWithPlayerShip = nonBulletObject == gameObject;
        if (collisionIsWithPlayerShip) {
            TakeDamage();
        }
        else if (nonBulletObject.TryGetComponent<CellHealthManager>(out var childCellHealthManager)) {
            if (childCellHealthManager.GetComponent<Cell>().State == CellState.Melded) {
                TakeDamage();
            }
            else {
                childCellHealthManager.TakeDamage();
            }
        }
    }

    public void TakeDamage() {
        AudioManager.Instance.PlayPlayerDamagedSound(1 + (4f - Health) / 8f);
        CameraManager.Instance.FlashDamageFilter();
        Health -= 1;
        if (Health <= 0) {
            Die();
        }
    }
}
