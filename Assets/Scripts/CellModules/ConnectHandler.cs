using System;
using System.Collections.Generic;
using UnityEngine;

public class ConnectHandler : CellModule {
    private const float _disconnectForceMagnitude = 500f;
    public void Connect(GameObject newParent) {
        transform.parent = newParent.transform;
        _teamTracker.ChangeTeam(Team.Player);
        Destroy(GetComponent<Rigidbody>());
    }

    public void Disconnect(Rigidbody oldParent) {
        _teamTracker.ChangeTeam(Team.Neutral);
        transform.parent = Containers.Cells;
        var rb = AddRigidbody();
        rb.linearVelocity = oldParent.linearVelocity;
        Vector3 disconnectForce = (transform.position - oldParent.transform.position) * _disconnectForceMagnitude;
        rb.AddForce(disconnectForce);
    }

    Rigidbody AddRigidbody() {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 15;
        rb.linearDamping = 2;
        rb.angularDamping = 5;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = false;
        return rb;
    }
}
