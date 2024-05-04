using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShipPiece : CellModule {
    public static event Action<GameObject> SignalAttachCell;

    private Dictionary<GameObject, float> _connectionTimeByCell = new();
    private const float _timeToConnect = .05f;
    public void OnCollisionEnter(Collision collision) {
        if (enabled == false) return;

        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer == Layers.NeutralCell) {
            _connectionTimeByCell[collision.gameObject] = Time.time + _timeToConnect;
        }
    }

    public void OnCollisionStay(Collision collision) {
        if (enabled == false) return;

        int collisionLayer = collision.gameObject.layer;
        if (collisionLayer == Layers.NeutralCell && Time.time >= _connectionTimeByCell[collision.gameObject]) {
            SignalAttachCell.Invoke(collision.gameObject);
            _connectionTimeByCell.Remove(collision.gameObject);
        }
    }

    protected override void HandleTeamChange(Team newTeam) {
        enabled = newTeam == Team.Player;
    }
}
