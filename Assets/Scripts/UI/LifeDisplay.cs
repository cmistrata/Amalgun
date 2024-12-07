using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class LifeDisplay : MonoBehaviour {
    public List<Image> ImageElements;
    public Sprite FullHeartSprite;
    public Sprite EmptyHeartSprite;

    public void Update() {
        int maxHealth = GameManager.GetMaxHealth();
        int currentHealth = GameManager.GetHealth();
        for (int i = 0; i < ImageElements.Count; i++) {
            if (i + 1 <= currentHealth) {
                ImageElements[i].enabled = true;
                ImageElements[i].sprite = FullHeartSprite;
            }
            else if (i + 1 <= maxHealth) {
                ImageElements[i].enabled = true;
                ImageElements[i].sprite = EmptyHeartSprite;
            }
            else {
                ImageElements[i].enabled = false;
            }
        }
    }
}
