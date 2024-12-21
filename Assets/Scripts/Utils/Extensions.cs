using System;
using System.Collections.Generic;
using System.Linq;
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

    public static List<GameObject> Descendants(this GameObject go) {
        List<GameObject> descendants = new();
        foreach (Transform child in go.transform) {
            AddDescendants(descendants, child);
        }
        return descendants;
    }

    private static void AddDescendants(List<GameObject> descendants, Transform current) {
        descendants.Add(current.gameObject);
        foreach (Transform child in current.transform) {
            AddDescendants(descendants, child);
        }
    }

    public static void DestroyChildren(this GameObject parent) {
        foreach (Transform child in parent.transform) {
            MonoBehaviour.Destroy(child.gameObject);
        }
    }

    public static void SetDistance(this GameObject target, GameObject from, float distance) {
        Vector3 fromToTarget = (target.Position() - from.Position()).normalized;
        target.transform.position = from.Position() + fromToTarget * distance;
    }

    public static void SetScale(this GameObject target, float scale) {
        Vector3 scaleVector = new Vector3(scale, scale, scale);
        target.transform.localScale = scaleVector;
    }

    public static T ChooseRandom<T>(this IEnumerable<T> e) {
        int selection = UnityEngine.Random.Range(0, e.Count());

        if (e is IReadOnlyList<T> eAsList) {
            return eAsList[selection];
        }
        else {
            int i = 0;
            foreach (T element in e) {
                if (i == selection) return element;
                i += 1;
            }
        }
        return e.First();
    }

    public static T ChooseRandomWeighted<T>(this IEnumerable<T> e, Func<T, float> randomness) {
        Dictionary<T, float> weightByObject = e.ToDictionary(t => t, t => randomness(t));
        float totalWeight = weightByObject.Values.Sum();
        float selection = UnityEngine.Random.Range(0, totalWeight);

        float passedWeight = 0;

        foreach (var tAndWeight in weightByObject) {
            var t = tAndWeight.Key;
            var tWeight = tAndWeight.Value;
            var nextPassedWeight = passedWeight + tWeight;
            if (selection <= nextPassedWeight) {
                return t;
            }
            passedWeight = nextPassedWeight;
        }

        return e.First();
    }

}
