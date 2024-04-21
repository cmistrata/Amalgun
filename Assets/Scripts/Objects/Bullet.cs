using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Bullet : MonoBehaviour
{
    public Team Team;
    public float TimeOutSeconds = 5f;
    protected float _lifetime = 0;
    private Rigidbody _rb;
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    void Update()
    {
        _lifetime += Time.deltaTime;

        float x = transform.position.x;
        float y = transform.position.y;

        //if (x > 32 || x < -32 || y > 20 || y < -20)
        //{
        //    Destroy(gameObject);
        //}

    }

    private void FixedUpdate()
    {
        if (_lifetime >= TimeOutSeconds && !_rb.useGravity)
        {
            _rb.useGravity = true;
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        Destroy(this.gameObject);
    }

    public void SetTimeout(float seconds)
    {
        TimeOutSeconds = seconds;
    }

    public void StartStraightMotion(Vector3 position, float motionAngle, float speed)
    {
        transform.position = position;
        transform.rotation = Quaternion.AngleAxis(motionAngle, Vector3.up);
        _rb.velocity = transform.forward * speed;
    }

    public void ChangeTeam(Team team)
    {
        Team = team;
        UpdateMesh();
        UpdateLayer();
    }

    private void UpdateMesh()
    {
        _meshRenderer.material = Team == Team.Enemy
            ? Globals.Instance.enemyBulletMaterial
            : Globals.Instance.playerBulletMaterial;
    }

    private void UpdateLayer()
    {
        gameObject.layer = Team == Team.Enemy
            ? Layers.EnemyBullet
            : Layers.PlayerBullet;
    }
}
