using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CellHealthManager))]
public class Player : MonoBehaviour {
    private Rigidbody _rb;
    private CellHealthManager _cellHealthManager;
    public static Player Instance;
    public int Health {
        get => _cellHealthManager.CurrentHealth;
    }

    public int MaxHealth {
        get => _cellHealthManager.MaxHealth;
    }

    private const float _torque = 5000f;
    private const float _newCellMassIncrease = 30f;

    public static event Action SignalPlayerDeath;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _cellHealthManager = GetComponent<CellHealthManager>();

        Instance = this;
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
        AudioManager.Instance.PlayPlayerDamagedSound(1 + (4f - _cellHealthManager.CurrentHealth) / 8f);
        CameraManager.Instance.FlashDamageFilter();
        _cellHealthManager.TakeDamage();
    }
}
