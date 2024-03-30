using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision) {
        Destroy(this.gameObject);
        GameManager.Instance.GainMoney();
    }
}
