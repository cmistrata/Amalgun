using System;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public CellState State;
    public float TimeOutSeconds = 5f;
    protected float _lifetime = 0;
    private Rigidbody _rb;
    public List<MeshRenderer> MeshRenderers;

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
    }
    void Update() {
        _lifetime += Time.deltaTime;
    }

    private void FixedUpdate() {
        if (_lifetime >= TimeOutSeconds && !_rb.useGravity) {
            _rb.useGravity = true;
        }
    }

    public void OnCollisionEnter(Collision other) {
        Destroy(this.gameObject);
    }

    public void SetTimeout(float seconds) {
        TimeOutSeconds = seconds;
    }

    public void StartStraightMotion(Vector3 position, float motionAngle, float speed) {
        transform.SetPositionAndRotation(position, Quaternion.AngleAxis(motionAngle, Vector3.up));
        _rb.linearVelocity = transform.forward * speed;
    }

    public void ChangeState(CellState state) {
        State = state;
        UpdateMeshes();
        UpdateLayer();
    }

    private void UpdateMeshes() {
        foreach (var meshRenderer in MeshRenderers) {
            meshRenderer.material = State == CellState.Enemy
                ? Globals.Instance.enemyBulletMaterial
                : Globals.Instance.playerBulletMaterial;
        }
    }

    private void UpdateLayer() {
        gameObject.layer = State == CellState.Enemy
            ? Layers.EnemyBullet
            : Layers.PlayerBullet;
    }
}
