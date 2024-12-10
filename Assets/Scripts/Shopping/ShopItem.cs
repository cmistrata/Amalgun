using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public enum ShopItemType {
    Melder = 0,
    SpeedUpgrade = 1
}

[RequireComponent(typeof(Collider))]
public class ShopItem : MonoBehaviour {
    public int Cost;
    public ShopItemType Type;
    public bool Selectable;
    public static event Action<ShopItem> SignalShopItemSelected;
    public static event Action<ShopItem> SignalShopItemBought;

    private void OnMouseDown() {
        if (Selectable) {
            Debug.Log($"Selecting1 {gameObject}");
            SignalShopItemSelected?.Invoke(this);
        }
        else if (GameManager.GetMoney() >= Cost) {
            SignalShopItemBought?.Invoke(this);
        }
    }
}
