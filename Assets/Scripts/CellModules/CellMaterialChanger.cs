using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TeamTracker))]
public class CellMaterialChanger : CellModule
{
    public Renderer Foundation;
    public Renderer Turret;

    protected override void ExtraAwake() {
        HandleTeamChange(_team);
    }

    private void UpdateMaterials(CellMaterials cellMaterials) {
        Foundation.material = cellMaterials.FoundationMaterial;
        Turret.material = cellMaterials.TurretMaterial;
    }

    //TODO: change this into a signal
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
