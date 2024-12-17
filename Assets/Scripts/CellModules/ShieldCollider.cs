using System;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCollider : MonoBehaviour {
    public event Action SignalShieldHit;
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == Layers.EnemyBullet || other.gameObject.layer == Layers.PlayerBullet) {
            SignalShieldHit?.Invoke();
            Destroy(other.gameObject);
        }

    }
}
