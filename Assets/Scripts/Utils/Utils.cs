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

    public static float DistanceBetween(GameObject gameObject1, GameObject gameObject2) {
        return (gameObject1.transform.position - gameObject2.transform.position).magnitude;
    }

    public static void SetMinimumDistance(GameObject baseObject, GameObject remoteObject, float minDistance) {
        Vector3 toRemote = remoteObject.Position() - baseObject.Position();
        if (toRemote.sqrMagnitude > minDistance * minDistance) {
            Vector3 remoteTargetPos = baseObject.Position() + minDistance * toRemote.normalized;
            remoteObject.transform.position = remoteTargetPos;
        }
    }

    public static T ParseEnum<T>(string value) {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    // public static HashSet<GameObject> FindNearbyGameObjects(GameObject gameObject, float distance, int layer, HashSet<GameObject> objectsToExclude = null) {
    //     objectsToExclude ??= new HashSet<GameObject>();
    //     HashSet<GameObject> nearbyObjects = new();

    //     Collider[] nearbyColliders = Physics.OverlapSphere(gameObject.transform.position, distance, Utils.GetLayerMask(Layers.PlayerCell));
    //     foreach (Collider nearbyCollider in nearbyColliders) {
    //         GameObject nearbyObject = nearbyCollider.gameObject;

    //         if (objectsToExclude.Contains(nearbyObject)) continue;
    //         if (_cellGraph.ContainsKey(nearbyObject)) {
    //             nearbyObjects.Add(nearbyObject);
    //         }
    //     }

    //     return nearbyObjects;
    // }

}
