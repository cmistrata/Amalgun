using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour {
    public static Globals Instance = null;
    public GameObject PlayerPrefab;

    [Header("Materials")]

    public CellMaterials friendlyCellMaterials;
    public CellMaterials neutralCellMaterials;
    public CellMaterials enemyCellMaterials;
    public CellMaterials meldedCellMaterials;

    public Material friendlyBulletMaterial;
    public Material enemyBulletMaterial;
    public Material meldedBulletMaterial;

    public const float ArenaWidth = 24f;
    public const float ArenaHeight = 24f;

    [Header("Cell Prefabs")]
    public GameObject BasicCellPrefab;
    public GameObject BasicCell2Prefab;
    public GameObject BasicCell3Prefab;

    public GameObject RocketCellPrefab;
    public GameObject RocketCell2Prefab;
    public GameObject RocketCell3Prefab;

    public GameObject MineCellPrefab;
    public GameObject MineCell2Prefab;
    public GameObject MineCell3Prefab;

    public GameObject TriCellPrefab;
    public GameObject TriCell2Prefab;
    public GameObject TriCell3Prefab;

    public GameObject ShieldCellPrefab;
    public GameObject ShieldCell2Prefab;
    public GameObject ShieldCell3Prefab;
    public static Dictionary<CellType, GameObject> CellPrefabByType;
    public static Dictionary<CellType, CellType> CellUpgradeByType = new() {
        {CellType.Basic, CellType.Basic2}, {CellType.Basic2, CellType.Basic3},
        {CellType.Rocket, CellType.Rocket2}, {CellType.Rocket2, CellType.Rocket3},
        {CellType.Mine, CellType.Mine2}, {CellType.Mine2, CellType.Mine3},
        {CellType.Tri, CellType.Tri2}, {CellType.Tri2, CellType.Tri3},
        {CellType.Shield, CellType.Shield2}, {CellType.Shield2, CellType.Shield3},
    };

    [Header("Shop Items")]
    public GameObject MeldUpgradePrefab;

    private void Awake() {
        Instance = this;
        CellPrefabByType = new Dictionary<CellType, GameObject>() {
            {CellType.Basic, BasicCellPrefab},
            {CellType.Basic2, BasicCell2Prefab},
            {CellType.Basic3, BasicCell3Prefab},

            {CellType.Rocket, RocketCellPrefab},
            {CellType.Rocket2, RocketCell2Prefab},
            {CellType.Rocket3, RocketCell3Prefab},

            {CellType.Mine, MineCellPrefab},
            {CellType.Mine2, MineCell2Prefab},
            {CellType.Mine3, MineCell3Prefab},

            {CellType.Tri, TriCellPrefab},
            {CellType.Tri2, TriCell2Prefab},
            {CellType.Tri3, TriCell3Prefab},

            {CellType.Shield, ShieldCellPrefab},
            {CellType.Shield2, ShieldCell2Prefab},
            {CellType.Shield3, ShieldCell3Prefab},
        };
    }
}
