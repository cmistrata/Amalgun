using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeamTracker))]
public class CellMaterialChanger : CellModule
{
    public Renderer Foundation;
    public Renderer Turret;

    private void Start() {
        HandleTeamChange(_team);
    }

    private void UpdateMaterials(CellMaterials cellMaterials) {
        Foundation.material = cellMaterials.FoundationMaterial;
        Turret.material = cellMaterials.TurretMaterial;
    }

    override protected void HandleTeamChange(Team newTeam) {
        if (Globals.Instance == null) return;
        switch (newTeam) {
            case Team.Player:
                UpdateMaterials(Globals.Instance.playerCellMaterials);
                break;
            case Team.Neutral:
                UpdateMaterials(Globals.Instance.neutralCellMaterials);
                break;
            case Team.Enemy:
                UpdateMaterials(Globals.Instance.enemyCellMaterials);
                break;
        }
    }
}
