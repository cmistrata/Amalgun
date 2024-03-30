using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletsContainer : MonoBehaviour {
    public static BulletsContainer Instance;
    private void Awake() {
        Instance = this;
    }
}
