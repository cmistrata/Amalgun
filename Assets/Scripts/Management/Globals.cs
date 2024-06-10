using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {
    public static Globals Instance = null;
    public GameObject PlayerPrefab;

    [Header("Materials")]

    public CellMaterials playerCellMaterials;
    public CellMaterials neutralCellMaterials;
    public CellMaterials enemyCellMaterials;

    public Material playerBulletMaterial;
    public Material enemyBulletMaterial;

    public const float ArenaWidth = 24f;
    public const float ArenaHeight = 24f;

    [Header("Cell Prefabs")]
    public GameObject BasicCellPrefab;
    public GameObject BasicCell2Prefab;
    public GameObject RocketCellPrefab;
    public static Dictionary<CellType, GameObject> CellPrefabByType;
    public static Dictionary<CellType, CellType> CellUpgradeByType = new() {
        {CellType.Basic, CellType.Basic2}
    };

    private void Awake() {
        Instance = this;
        CellPrefabByType = new Dictionary<CellType, GameObject>() {
            {CellType.Basic, BasicCellPrefab},
            {CellType.Basic2, BasicCell2Prefab},
            {CellType.Rocket, RocketCellPrefab},
        };
    }
}
