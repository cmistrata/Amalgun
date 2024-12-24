using UnityEngine;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance;
    public GameObject PauseMenu;
    public bool Paused = false;
    public void Awake() {
        Instance = this;
    }

    public void Update() {
        if (GameManager.Instance.State == GameState.Fighting || GameManager.Instance.State == GameState.Shop) {
            CheckForPause();
        }
    }

    public void Pause() {
        Paused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        PauseMenu.SetActive(true);
    }

    public void Unpause() {
        Paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        PauseMenu.SetActive(false);
    }

    void CheckForPause() {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) {
            Paused = !Paused;
            if (Paused) {
                Pause();
            }
            else {
                Unpause();
            }
        }
    }
}
