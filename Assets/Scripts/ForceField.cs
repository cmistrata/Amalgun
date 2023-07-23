using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{

    public float MaxHealth = 2;
    private float currentHealth;
    private ShieldPart shieldPart;


    // Start is called before the first frame update
    void Start()
    {
        shieldPart = GetComponentInParent<ShieldPart>();
        currentHealth = MaxHealth;
    }

    public void RestoreShield()
    {
        currentHealth = MaxHealth;
        AudioManager.Instance.PlayShieldReactivateSound();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            AudioManager.Instance.PlayShieldDownSound();
            shieldPart.DisableShield();
        }
        else
        {
            AudioManager.Instance.PlayShieldHitSound();
        }
    }
}
