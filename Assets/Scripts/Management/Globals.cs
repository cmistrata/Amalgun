using System;
using System.Collections.Generic;
using System.Linq;
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
    // Cell globals instantiated via reflection, looking at the "Resources/Cells" folder and parsing
    // names there.
    public static GameObject[] Cells;
    public static Dictionary<CellType, GameObject> CellPrefabByType;
    public static Dictionary<CellType, CellType> CellUpgradeByType = new();

    [Header("Shop Items")]
    public GameObject MeldUpgradePrefab;

    public static Dictionary<CellType, CellStats> StatsByCellType = new() {
        {CellType.Basic, new CellStats(difficulty: 1, rarity: 1)},
        {CellType.Mine, new CellStats(difficulty: 1, rarity:  8)},
        {CellType.Rocket, new CellStats(difficulty: 4, rarity: 12)},
        {CellType.Shield, new CellStats(difficulty: 1, rarity: 12)},
        {CellType.Gatling, new CellStats(difficulty: 7, rarity: 12)},
        {CellType.Tri, new CellStats(difficulty: 2, rarity: 6)},
    };

    private void Awake() {
        Instance = this;
        if (Cells == null) {
            var CellPrefabs = Resources.LoadAll<GameObject>("Cells");
            CellPrefabByType = CellPrefabs
                .ToDictionary(
                    cell => Utils.ParseEnum<CellType>(cell.name.Replace("Cell", "")),
                    cell => cell
                );
            foreach (CellType cellType in CellPrefabByType.Keys) {
                var cellTypeStr = cellType.ToString();
                var cellTypeLastChar = cellTypeStr[^1];
                string nextCellTypeStr = "";
                if (!char.IsNumber(cellTypeLastChar)) {
                    nextCellTypeStr = $"{cellTypeStr}2";
                }
                else {
                    nextCellTypeStr = $"{cellTypeStr[..^1]}{(char)(cellTypeLastChar + 1)}";
                }
                if (Enum.TryParse(nextCellTypeStr, out CellType nextCellType)) {
                    CellUpgradeByType[cellType] = nextCellType;
                }
            }
        }
    }
}
