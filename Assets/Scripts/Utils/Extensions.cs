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

    public static Vector3 Position(this GameObject go) {
        return go.transform.position;
    }

    public static Cell Cell(this GameObject go) {
        if (go.TryGetComponent<Cell>(out var cellComponent)) {
            return cellComponent;
        }
        else {
            return null;
        }
    }

    public static Rigidbody Rigidbody(this GameObject go) {
        if (go.TryGetComponent<Rigidbody>(out var rigidbody)) {
            return rigidbody;
        }
        else {
            return null;
        }
    }

    public static void SetDistance(this GameObject target, GameObject from, float distance) {
        Vector3 fromToTarget = (target.Position() - from.Position()).normalized;
        target.transform.position = from.Position() + fromToTarget * distance;
    }
}
