using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreenResolution : MonoBehaviour
{
    void Start() {
        Screen.SetResolution(2, 1, true);
    }
}
