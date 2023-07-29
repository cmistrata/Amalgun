using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPart : Part
{

    public bool ShieldsUp = true;
    private ForceField forceField;

    public float MaxShieldHealth = 1;
    private float currentShieldHealth;

    public Sprite EnemyField;
    public Sprite PlayerField;

    public float ShieldRechargeTime = 5f;
    private float _timeUntilShieldRecharge = 5f;

    public override void Start()
    {
        forceField = transform.GetComponentInChildren<ForceField>();
        base.Start();
        ShieldsUp = true;
        _timeUntilShieldRecharge = ShieldRechargeTime;
        currentShieldHealth = MaxShieldHealth;
    }

    public void Update()
    {

        // Check if this is part of a player that is shooting
        Player player = gameObject.GetComponentInParent<Player>();
        if (player != null && _timeUntilShieldRecharge < 0)
        {
            EnableShield();
            _timeUntilShieldRecharge = ShieldRechargeTime;
        }

        // Check if this is part of an enemy
        EnemyController enemy = gameObject.GetComponentInParent<EnemyController>();
        if (enemy != null && _timeUntilShieldRecharge < 0)
        {
            EnableShield();
            _timeUntilShieldRecharge = ShieldRechargeTime;
        }

        if (!ShieldsUp)
        {
            _timeUntilShieldRecharge -= Time.deltaTime;
        }
    }

    public void EnableShield()
    {
        forceField.gameObject.SetActive(true);
        forceField.RestoreShield();
        ShieldsUp = true;
    }

    public void DisableShield()
    {
        forceField.gameObject.SetActive(false);
        ShieldsUp = false;
    }
    override public void ConvertEnemyPart()
    {
        base.ConvertEnemyPart();
        forceField.gameObject.layer = LayerMask.NameToLayer("PlayerForceField");
    }
}
