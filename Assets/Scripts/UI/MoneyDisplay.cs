using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class MoneyDisplay : MonoBehaviour {
    public TMP_Text Text;

    public void Update() {
        Text.text = $"{GameManager.GetMoney()}";
    }
}
