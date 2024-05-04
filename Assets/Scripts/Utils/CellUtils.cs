using UnityEngine;

public static class CellUtils {
    public static CellType GetCellType(GameObject obj) {
        return obj.GetComponent<CellProperties>().Type;
    }

    public static void ConvertToTeam(GameObject cell, Team team) {
        cell.GetComponent<TeamTracker>().ChangeTeam(team);
    }

    public static void EnableMovement(GameObject cell) {
        cell.GetComponent<MovementBase>().enabled = true;
    }
}
