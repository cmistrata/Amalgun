using System;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    static Dictionary<String, int> _lastLogsecondByMessage = new Dictionary<String, int>();

    public static bool MouseRaycast(out RaycastHit hit) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hit, 100);
    }

    public static void LogOncePerSecond(string logMessage, string key = null) {
        key ??= logMessage[..2];
        if (!_lastLogsecondByMessage.ContainsKey(key) || _lastLogsecondByMessage[key] < DateTime.Now.Second) {
            _lastLogsecondByMessage[key] = DateTime.Now.Second;
            Debug.Log(logMessage);
        }
    }
}