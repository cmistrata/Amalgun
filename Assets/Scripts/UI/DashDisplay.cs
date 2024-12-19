using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class DashDisplay : MonoBehaviour {
    public List<Image> ImageElements;
    public Sprite FullDashSprite;
    public Sprite EmptyDashSprite;

    public void Update() {
        int maxDashes = GameManager.GetMaxDashes();
        int currentDashes = GameManager.GetCurrentDashes();
        for (int i = 0; i < ImageElements.Count; i++) {
            if (i + 1 <= currentDashes) {
                ImageElements[i].enabled = true;
                ImageElements[i].sprite = FullDashSprite;
            }
            else if (i + 1 <= maxDashes) {
                ImageElements[i].enabled = true;
                ImageElements[i].sprite = EmptyDashSprite;
            }
            else {
                ImageElements[i].enabled = false;
            }
        }
    }
}
