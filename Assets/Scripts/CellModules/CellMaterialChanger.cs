using UnityEngine;

[RequireComponent(typeof(Cell))]
public class CellMaterialChanger : CellModule {
    public Renderer Foundation;
    public Renderer Turret;

    private void Start() {
        HandleStateChange(_state);
    }

    private void UpdateMaterials(CellMaterials cellMaterials) {
        Foundation.sharedMaterial = cellMaterials.FoundationMaterial;
        Turret.sharedMaterial = cellMaterials.TurretMaterial;
    }

    //TODO: change this into a signal
    override protected void HandleStateChange(CellState newState) {
        if (Globals.Instance == null) return;
        switch (newState) {
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
