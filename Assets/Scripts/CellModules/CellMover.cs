using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellMover : CellModule {

    public BaseMovement Movement;
    private Rigidbody _rb;

    protected override void ExtraAwake() 
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    public void FixedUpdate() 
    {
        if (_rb != null)
        {
            Movement.ApplyMovement(_rb, Time.deltaTime);
        }
    }

    protected override void HandleTeamChange(Team newTeam)
    {
        enabled = newTeam == Team.Enemy;
    }
}
