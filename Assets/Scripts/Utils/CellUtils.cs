using UnityEngine;

public static class CellUtils {
    public static CellType GetCellType(GameObject obj) {
        return obj.GetComponent<Cell>().Type;
    }

    public static void ConvertToState(GameObject cell, CellState state) {
        cell.GetComponent<Cell>().ChangeState(state);
    }

    public static void EnableMovement(GameObject cell) {
        cell.GetComponent<MovementBase>().enabled = true;
    }
}
