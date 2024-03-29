using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static Globals Instance = null;

    private void Awake() {
        Instance = this;
    }

    public CellMaterials playerCellMaterials;
    public CellMaterials neutralCellMaterials;
    public CellMaterials enemyCellMaterials;

    public Material playerBulletMaterial;
    public Material enemyBulletMaterial;
}
