using UnityEngine;

public class Globals : MonoBehaviour {
    public static Globals Instance = null;

    private void Awake() {
        Instance = this;
    }

    public GameObject PlayerPrefab;

    public CellMaterials playerCellMaterials;
    public CellMaterials neutralCellMaterials;
    public CellMaterials enemyCellMaterials;

    public Material playerBulletMaterial;
    public Material enemyBulletMaterial;

    public const float ArenaWidth = 24f;
    public const float ArenaHeight = 24f;
}
