using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectsContainer : MonoBehaviour
{
    public static GameObjectsContainer Instance;
    public Transform bulletsContainer;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
}
