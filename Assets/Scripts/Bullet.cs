using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    public bool isPlayerBullet = false;
    // Assigned by the shooter
    public int Damage = 0;

    public float TimeOutSeconds = 5f;
    protected float lifetime = 0;
    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= TimeOutSeconds)
        {
            Destroy(this.gameObject);
        }

        float x = transform.position.x;
        float y = transform.position.y;

        if (x > 32 || x < -32 || y > 20 || y < -20)
        {
            Destroy(gameObject);
        }

    }

    /// <summary>
    /// Damages the entity hit when applicable, destroys the bullet when hitting a part.
    /// </summary>
    /// <param name="other"></param>
    public void OnCollisionEnter2D(Collision2D other) {
        Destroy(this.gameObject);
        if (other.gameObject.TryGetComponent<ForceField>(out var field)) {
            if (!isPlayerBullet && other.gameObject.layer == LayerMask.NameToLayer("PlayerForceField") ||
                isPlayerBullet && other.gameObject.layer == LayerMask.NameToLayer("EnemyForceField")) {
                field.TakeDamage(Damage);
            }
        }

        if (!other.gameObject.TryGetComponent<Part>(out var part)) {
            return;
        }
        // Cause damage if it hits an opponent (player bullet hits enemy, or enemy bullet hits player)
        else if ((isPlayerBullet && part.Team == Team.Enemy) || (!isPlayerBullet && part.Team == Team.Player)) {
            part.TakeDamage(Damage);
        }
    }
}
