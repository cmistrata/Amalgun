using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAtPlayerEnemy : EnemyController
{
    public void Start() {
        Shooting = true;
    }

    public override Vector3 GetShootingTarget()
    {
        return Player.Instance != null ? Player.Instance.transform.position : Vector3.zero;
    }
}
