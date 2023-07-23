using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This part needs custom times, so the shooting is handled in here

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class RailgunPart : Part
{
    public float chargeTime = .2f;
    public float MaxLineWidth = .08f;
    private float chargingTimer = 0f;
    private bool charging = false;
    private Vector3 shootingDirection;
    private LineRenderer lineRenderer;
    private ContactFilter2D noTriggerFilter;

    public new void Start()
    {
        base.Start();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 1;
        lineRenderer.endWidth = 1;
        lineRenderer.widthMultiplier = 0;

        noTriggerFilter = new ContactFilter2D();
        noTriggerFilter.useTriggers = false;
        noTriggerFilter.useLayerMask = true;
        noTriggerFilter.layerMask = LayerMask.GetMask(new string[] { "Default", "PlayerPart", "EnemyPart" });
    }

    public new void Update()
    {
        // Check if this is part of a player that is shooting or charging
        Player player = gameObject.GetComponentInParent<Player>();
        if (player != null && player.Shooting && shotTimer < 0)
        {
            charging = true;
            chargingTimer = chargeTime;
            shotTimer = shotInterval;
            lineRenderer.widthMultiplier = 0;
            lineRenderer.enabled = true;
        }

        if (player != null && charging)
        {
            // Linecast to target
            shootingDirection = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            //SwivelTurret(Input.mousePosition);
            Vector3 shootingTarget = transform.position + (50 * shootingDirection);
            List<RaycastHit2D> hitInfo = new List<RaycastHit2D>();
            if (Physics2D.Linecast(transform.position, shootingTarget, noTriggerFilter, hitInfo) > 0)
            {
                // Something was hit in this linecast. Check if this is an enemy
                System.Predicate<RaycastHit2D> enemyTest = new System.Predicate<RaycastHit2D>(hit => hit.transform.gameObject.GetComponentInParent<EnemyController>());
                if (hitInfo.Exists(enemyTest))
                {
                    RaycastHit2D firstHit = hitInfo.Find(enemyTest);
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, firstHit.point);
                }
                else
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, shootingTarget);
                }
            }
            else
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, shootingTarget);
            }

            // Update line width based on charge amount
            lineRenderer.widthMultiplier = MaxLineWidth * ((chargeTime - chargingTimer) / chargeTime);

            // Weapon is charged, fire
            if (chargingTimer < 0)
            {
                //ShootProjectile(Input.mousePosition, ProjectilePrefab, true);
                charging = false;
            }
            chargingTimer -= Time.deltaTime;
        }

        // Check if this is part of an enemy
        EnemyController enemy = gameObject.GetComponentInParent<EnemyController>();
        if (enemy != null && enemy.Shooting && shotTimer < 0)
        {
            charging = true;
            lineRenderer.enabled = true;
            chargingTimer = chargeTime;
            shotTimer = shotInterval;
            // Enemies lock in target direction so they are predictable
            shootingDirection = (enemy.GetShootingTarget() - transform.position).normalized;
            //SwivelTurret(Camera.main.WorldToScreenPoint(transform.position + shootingDirection));
        }

        if (enemy != null && charging)
        {
            // linecast to target
            Vector3 shootingTarget = transform.position + 50 * shootingDirection;
            List<RaycastHit2D> hitInfo = new List<RaycastHit2D>();
            if (Physics2D.Linecast(transform.position, shootingTarget, noTriggerFilter, hitInfo) > 0)
            {
                // Something was hit in this linecast. Check if this is an enemy
                System.Predicate<RaycastHit2D> playerTest = new System.Predicate<RaycastHit2D>(hit => hit.transform.gameObject.GetComponentInParent<Player>());
                if (hitInfo.Exists(playerTest))
                {
                    RaycastHit2D firstHit = hitInfo.Find(playerTest);
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, firstHit.point);
                }
                else
                {
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, shootingTarget);
                }
            }
            else
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, shootingTarget);
            }

            // Update line width based on charge amount
            lineRenderer.widthMultiplier = MaxLineWidth * ((chargeTime - chargingTimer) / chargeTime);

            // Weapon is charged, fire
            if (chargingTimer < 0)
            {
                //ShootProjectile(Camera.main.WorldToScreenPoint(transform.position + shootingDirection), ProjectilePrefab, false);
                charging = false;
            }
            chargingTimer -= Time.deltaTime;
        }

        shotTimer -= Time.deltaTime;
    }
    //protected override void ShootProjectile(Vector3 screenPointTarget, GameObject projectilePrefab, bool isPlayerBullet)
    //{
    //    // Cast a line in the direction shot, hit first thing collided with
    //    List<RaycastHit2D> hitInfo = new List<RaycastHit2D>();
    //    Vector3 dir = (screenPointTarget - Camera.main.WorldToScreenPoint(transform.position)).normalized;


    //    if (Physics2D.Linecast(transform.position, transform.position + dir * 50, noTriggerFilter, hitInfo) > 0)
    //    {
    //        // Something was hit in this linecast. Check if this is an enemy
    //        System.Predicate<RaycastHit2D> collisionTest;
    //        if (isPlayerBullet)
    //        {
    //            collisionTest = new System.Predicate<RaycastHit2D>(hit => hit.transform.gameObject.GetComponentInParent<Enemy>());
    //        }
    //        else
    //        {
    //            collisionTest = new System.Predicate<RaycastHit2D>(hit => hit.transform.gameObject.GetComponentInParent<Player>());
    //        }
    //        if (hitInfo.Exists(collisionTest))
    //        {
    //            RaycastHit2D firstHit = hitInfo.Find(collisionTest);
    //            Part part = firstHit.collider.gameObject.GetComponent<Part>();
    //            if (part == null)
    //            {
    //                lineRenderer.enabled = false;
    //                Debug.LogWarning("Railgun hit something that wasn't a part");
    //                return;
    //            }
    //            else
    //            {
    //                part.TakeDamage(BulletDamage);
    //            }
    //        }
    //    }

    //    gameObject.GetComponent<AudioSource>().Play();
    //    lineRenderer.enabled = false;
    //}

    public override void ConvertEnemyPart()
    {
        lineRenderer.widthMultiplier = 0;
        lineRenderer.enabled = false;
        charging = false;
        base.ConvertEnemyPart();
    }
}
