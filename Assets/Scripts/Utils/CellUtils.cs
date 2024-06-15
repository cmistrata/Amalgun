using UnityEngine;

public static class CellUtils {
    public static CellType GetCellType(GameObject obj) {
        return obj.GetComponent<Cell>().Type;
    }

    public static void ConvertToTeam(GameObject cell, CellState team) {
        cell.GetComponent<Cell>().ChangeState(team);
    }

    public static void EnableMovement(GameObject cell) {
        cell.GetComponent<MovementBase>().enabled = true;
    }
}
