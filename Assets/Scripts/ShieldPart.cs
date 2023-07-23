using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPart : Part
{

    public bool shieldsUp;
    private ForceField forceField;

    public float MaxShieldHealth = 100;
    private float currentShieldHealth;

    public Sprite EnemyField;
    public Sprite PlayerField;

    public override void Start()
    {
        forceField = transform.GetComponentInChildren<ForceField>();
        base.Start();
        shieldsUp = true;
        shotTimer = shotInterval;
        currentShieldHealth = MaxShieldHealth;
    }

    override public void Update()
    {

        // Check if this is part of a player that is shooting
        Player player = gameObject.GetComponentInParent<Player>();
        if (player != null && shotTimer < 0)
        {
            EnableShield();
            shotTimer = shotInterval;
        }

        // Check if this is part of an enemy
        EnemyController enemy = gameObject.GetComponentInParent<EnemyController>();
        if (enemy != null && shotTimer < 0)
        {
            EnableShield();
            shotTimer = shotInterval;
        }

        if (!shieldsUp)
        {
            shotTimer -= Time.deltaTime;
        }
    }

    public void EnableShield()
    {
        forceField.gameObject.SetActive(true);
        forceField.RestoreShield();
        shieldsUp = true;
    }

    public void DisableShield()
    {
        forceField.gameObject.SetActive(false);
        shieldsUp = false;
    }
    override public void ConvertEnemyPart()
    {
        base.ConvertEnemyPart();
        forceField.gameObject.layer = LayerMask.NameToLayer("PlayerForceField");
    }

    override protected void UpdateSprites()
    {
        base.UpdateSprites();
        if (state == PartState.Enemy)
        {
            forceField.gameObject.GetComponent<SpriteRenderer>().sprite = EnemyField;
        }
        else
        {
            forceField.gameObject.GetComponent<SpriteRenderer>().sprite = PlayerField;
        }
    }
}
