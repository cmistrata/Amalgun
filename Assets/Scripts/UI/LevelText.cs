using UnityEngine;
using TMPro;

public class LevelText : MonoBehaviour {
    public TMP_Text Text;
    private int _level = 0;
    private int _wave = 0;

    public void Start() {
        _level = GameManager.Instance != null ? GameManager.Instance.Level0Indexed : -1;
        UpdateText();
    }

    public void Update() {
        int newLevel = GameManager.Instance != null ? GameManager.Instance.Level0Indexed : -1;
        int newWave = GameManager.Instance != null ? GameManager.Instance.Wave0Indexed : -1;
        if (newLevel != _level || _wave != newWave) {
            _level = newLevel;
            _wave = newWave;
            UpdateText();
        }
    }

    void UpdateText() {
        if (GameManager.Instance.State == GameState.Fighting) {
            Text.text = $"level {_level + 1} wave {_wave + 1}";
        }
        else if (GameManager.Instance.State == GameState.Shop) {
            Text.text = $"level {_level + 1} shop";
        }
        else {
            Text.text = "";
        }
    }
}
