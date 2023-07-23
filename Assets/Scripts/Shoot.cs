using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public float ProjectileVelocity = 1;
    public float InitialProjectileOffset = 0;
    public float ShotsPerSecond = 2;
    private float shotInterval;
    private float shotTimer = 0;

    // Start is called before the first frame update
    void Start() {
        shotInterval = 1/ShotsPerSecond;
    }

    // Update is called once per frame
    void Update() {

        if(Input.GetMouseButton(0)) {
            if (shotTimer <= 0) {
                ShootProjectile(Input.mousePosition);
                shotTimer = shotInterval;
            }
        }

        shotTimer -= Time.deltaTime;
    }

/// <summary>
/// Instatiates a projectile with InitialProjectileOffset from the transform in the direction of the cursor.
/// Sets the projectile moving towards the cursor with ProjectileVelocty speed.
/// </summary>
    private void ShootProjectile(Vector3 screenPointTarget) {
        Vector3 dir = (screenPointTarget - Camera.main.WorldToScreenPoint(transform.position)).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject projectile = Instantiate(ProjectilePrefab, transform.position + (InitialProjectileOffset*dir), 
                                            Quaternion.AngleAxis(angle, Vector3.forward));
        projectile.GetComponent<Rigidbody2D>().velocity = dir * ProjectileVelocity;
    }
}
