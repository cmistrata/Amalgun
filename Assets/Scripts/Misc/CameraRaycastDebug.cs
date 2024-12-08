using UnityEngine;

public class CameraRaycastDebug : MonoBehaviour {
    public GameObject DebugCursor;
    void Update() {
        Utils.MouseRaycast(out var hit, Layers.PlayerCell);
        DebugCursor.transform.position = hit.point;
    }
}
