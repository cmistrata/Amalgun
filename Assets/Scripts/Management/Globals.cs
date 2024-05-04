using UnityEngine;

public class Globals : MonoBehaviour {
    public static Globals Instance = null;

    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public CellMaterials playerCellMaterials;
    public CellMaterials neutralCellMaterials;
    public CellMaterials enemyCellMaterials;

    public Material playerBulletMaterial;
    public Material enemyBulletMaterial;

    public const float ArenaWidth = 28f;
    public const float ArenaHeight = 18f;
}
