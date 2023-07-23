using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : EnemyController
{
    public void Start() {
        Shooting = true;
    }

    public override Vector3 GetShootingTarget()
    {
        return new Vector3(0,0,0);
    }
}
