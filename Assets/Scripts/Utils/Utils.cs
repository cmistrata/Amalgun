using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    static Dictionary<string, int> _lastLogSecondByMessage = new();

    public static bool MouseRaycast(out RaycastHit hit, int layer = -1) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (layer == -1) {
            return Physics.Raycast(ray, out hit, 30);
        }
        else {
            return Physics.Raycast(ray, out hit, 30, GetLayerMask(layer));
        }
    }

    public static Vector3 GetMousePosition() {
        MouseRaycast(out var raycastHit, Layers.MouseAimCollider);
        var mousePosition = new Vector3(raycastHit.point.x, 0, raycastHit.point.z);
        return mousePosition;
    }

    public static int GetLayerMask(int layer) {
        return 1 << layer;
    }

    public static void LogOncePerSecond(string logMessage, string key = null) {
        key ??= logMessage[..2];
        if (!_lastLogSecondByMessage.ContainsKey(key) || _lastLogSecondByMessage[key] < DateTime.Now.Second) {
            _lastLogSecondByMessage[key] = DateTime.Now.Second;
            Debug.Log(logMessage);
        }
    }

    public static IList<T> Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}
