using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(SpriteRenderer))]
public class Rocket : Bullet
{
    public float startupLength = .5f;
    private bool inStartupPhase = true;
    public float thrustStrength = 1000f;

    public float startupRotationSpeed = 20f;
    public float thrustingRotationSpeed = 5f;

    public float MaxSpeed = 8f;



    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= TimeOutSeconds)
        {
            Destroy(this.gameObject);
        }

        if (inStartupPhase)
        {
            if (lifetime >= startupLength)
            {
                inStartupPhase = false;
                gameObject.GetComponent<Animator>().SetBool("IsEnemy", !isPlayerBullet);
                gameObject.GetComponent<Animator>().enabled = true;
                gameObject.GetComponent<AudioSource>().Play();
            }
            else
            {
                return;
            }
        }



    }

    void FixedUpdate()
    {
        var rotationSpeed = inStartupPhase ? startupRotationSpeed : thrustingRotationSpeed;
        var rigidbody = gameObject.GetComponent<Rigidbody2D>();
        var target = isPlayerBullet ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Player.Instance.transform.position;
        Vector3 targetDirection = (target - transform.position).normalized;

        // Rotate the rocket towards the target
        var angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        var targetRotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        var angleBetweenTarget = Quaternion.Angle(transform.rotation, targetRotation);
        if (angleBetweenTarget < rotationSpeed)
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
        }

        if (!inStartupPhase)
        {
            // Apply force in the direction the rocket is facing
            rigidbody.AddForce(transform.up * thrustStrength);
            rigidbody.velocity = transform.up * Mathf.Min(rigidbody.velocity.magnitude, MaxSpeed);
        }
    }
}
