using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public static class Utils {
    static Dictionary<string, int> _lastLogsecondByMessage = new();

    public static bool MouseRaycast(out RaycastHit hit, int layer = -1) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (layer == -1) {
            return Physics.Raycast(ray, out hit, 30);
        } else {
            return Physics.Raycast(ray, out hit, 30, GetLayerMask(layer));
        }
    }

    public static int GetLayerMask(int layer) {
        return 1 << layer;
    }

    public static Vector3 GetMousePosition() {
        MouseRaycast(out RaycastHit hit);
        return hit.point;
    }

    public static void LogOncePerSecond(string logMessage, string key = null) {
        key ??= logMessage[..2];
        if (!_lastLogsecondByMessage.ContainsKey(key) || _lastLogsecondByMessage[key] < DateTime.Now.Second) {
            _lastLogsecondByMessage[key] = DateTime.Now.Second;
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