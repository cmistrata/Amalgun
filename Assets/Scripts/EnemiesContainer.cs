using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesContainer : MonoBehaviour
{
    public static EnemiesContainer Instance;

    private void Awake() {
        Instance = this;
    }
}
