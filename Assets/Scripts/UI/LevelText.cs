using UnityEngine;
using TMPro;

public class LevelText : MonoBehaviour {
    public TMP_Text Text;
    private int _level = 0;

    public void Start() {
        _level = GameManager.Instance != null ? GameManager.Instance.LevelNumber0Indexed : -1;
        UpdateText();
    }

    public void Update() {
        int newLevel = GameManager.Instance != null ? GameManager.Instance.LevelNumber0Indexed : -1;
        if (newLevel != _level) {
            _level = newLevel;
            UpdateText();
        }
    }

    void UpdateText() {
        Text.text = $"Level {_level + 1}";
    }
}
