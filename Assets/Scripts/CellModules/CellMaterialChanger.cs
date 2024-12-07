using UnityEngine;

[RequireComponent(typeof(Cell))]
public class CellMaterialChanger : CellModule {
    public Renderer Foundation;
    public Renderer Turret;

    private void Start() {
        HandleTeamChange(_team);
    }

    private void UpdateMaterials(CellMaterials cellMaterials) {
        Foundation.sharedMaterial = cellMaterials.FoundationMaterial;
        Turret.sharedMaterial = cellMaterials.TurretMaterial;
    }

    //TODO: change this into a signal
    override protected void HandleTeamChange(CellState newTeam) {
        if (Globals.Instance == null) return;
        switch (newTeam) {
            case CellState.Player:
            case CellState.BeingAbsorbed:
            case CellState.Absorbing:
            case CellState.Attaching:
                UpdateMaterials(Globals.Instance.playerCellMaterials);
                break;
            case CellState.Neutral:
                UpdateMaterials(Globals.Instance.neutralCellMaterials);
                break;
            case CellState.Enemy:
                UpdateMaterials(Globals.Instance.enemyCellMaterials);
                break;
        }
    }
}
