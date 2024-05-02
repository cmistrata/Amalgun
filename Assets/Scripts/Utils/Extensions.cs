using System.Collections;
using System.Collections.Generic;
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
}
