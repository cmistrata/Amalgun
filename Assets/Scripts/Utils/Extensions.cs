using UnityEngine;

public static class Extensions {
    public static void SetLayerRecursive(this GameObject gameObject, int layer) {
        gameObject.layer = layer;
        foreach (Transform child in gameObject.transform) {
            child.gameObject.layer = layer;

            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                child.gameObject.SetLayerRecursive(layer);
        }
    }

    public static Vector3 UpdateCoords(this Vector3 vector3, float? x = null, float? y = null, float? z = null) {
        if (x != null) {
            vector3.x = (float)x;
        }
        if (y != null) {
            vector3.y = (float)y;
        }
        if (z != null) {
            vector3.z = (float)z;
        }
        return vector3;
    }
}
