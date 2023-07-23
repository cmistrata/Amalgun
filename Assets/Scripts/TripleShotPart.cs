using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleShotPart : Part
{
    public int degreeSpread = 30;

    //override protected void ShootProjectile(Vector3 screenPointTarget, GameObject projectilePrefab, bool isPlayerBullet) {
    //    Vector3 dir = (screenPointTarget - Camera.main.WorldToScreenPoint(transform.position)).normalized;
    //    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

    //    GameObject[] projectiles = new GameObject[3];
    //    projectiles[0] = Instantiate(projectilePrefab, transform.position + (InitialProjectileOffset * dir),
    //                                        Quaternion.AngleAxis(angle, Vector3.forward), transform?.parent?.parent);
    //    projectiles[1] = Instantiate(projectilePrefab, transform.position + (InitialProjectileOffset * dir),
    //                                        Quaternion.AngleAxis(angle + 30, Vector3.forward), transform?.parent?.parent);
    //    projectiles[2] = Instantiate(projectilePrefab, transform.position + (InitialProjectileOffset * dir),
    //                                        Quaternion.AngleAxis(angle - 30, Vector3.forward), transform?.parent?.parent);

    //    Debug.Log("Triple Shot!");

    //    foreach (GameObject projectile in projectiles) {
    //        Bullet bullet = projectile.GetComponent<Bullet>();
    //        SpriteRenderer sRender = bullet.gameObject.GetComponent<SpriteRenderer>();
    //        sRender.sprite = (state == PartState.Enemy ? EnemyBulletSprite : PlayerBulletSprite);

    //        bullet.Damage = BulletDamage;
    //        bullet.TimeOutSeconds = BulletTtlSeconds;
    //        bullet.isPlayerBullet = isPlayerBullet;
    //        projectile.GetComponent<Rigidbody2D>().velocity = projectile.transform.right * ProjectileVelocity;
    //    }
    //    gameObject.GetComponent<AudioSource>().Play();
    //}
}
