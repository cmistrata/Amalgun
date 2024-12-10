using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;


public class Shop : MonoBehaviour {

    public void Appear() {
        RefreshItems();
        gameObject.SetActive(true);
    }

    public void Disappear() {
        gameObject.SetActive(false);
    }

    public void RefreshItems() {
        gameObject.DestroyChildren();
        Instantiate(Globals.Instance.MeldUpgradePrefab, parent: transform);
    }
}
