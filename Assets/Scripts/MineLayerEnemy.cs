using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineLayer : EnemyController
{
    public void Start() {
        Shooting = true;
    }

    public override Vector3 GetShootingTarget()
    {
        return transform.position;
    }
}
