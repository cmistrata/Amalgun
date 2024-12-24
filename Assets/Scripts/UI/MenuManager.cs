using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum Menu {
    GameOver,
    Pause,
    MainMenu
}

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance;
    public GameObject PauseMenu;
    public GameObject MainMenu;
    public GameObject GameOverMenu;
    public GameObject BattleUI;


    public bool Paused = false;
    private GameObject _activeMenu = null;

    public Dictionary<Menu, GameObject> MenuByEnum;
    public void Awake() {
        Instance = this;
        MenuByEnum = new() {
            {Menu.GameOver, GameOverMenu},
            {Menu.Pause, PauseMenu},
            {Menu.MainMenu, MainMenu}
        };
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
    }

    public void Unpause() {
        Paused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (_activeMenu == PauseMenu) {
            UnloadMenuObject();
        }
    }

    void CheckForPause() {
        if (GameManager.Instance.State == GameState.MainMenus) {
            return;
        }
        if (Input.GetKeyDown(KeyCode.P) || Actions.Cancel.WasPressedThisFrame()) {
            Paused = !Paused;
            if (Paused) {
                Pause();
                LoadMenuObject(PauseMenu);
            }
            else {
                Unpause();
            }
        }
    }

    public static void SetBattleUIActive(bool active) {
        Instance.BattleUI.SetActive(active);
    }

    public static void LoadMenu(Menu menu) {
        Instance.LoadMenuObject(Instance.MenuByEnum[menu]);
    }

    void LoadMenuObject(GameObject menu) {
        if (_activeMenu != null) {
            _activeMenu.SetActive(false);
        }
        menu.SetActive(true);
        _activeMenu = menu;
        if (GameManager.Instance.State == GameState.MainMenus) {
            Unpause();
        }
    }

    public void UnloadMenuObject() {
        _activeMenu.SetActive(false);
        _activeMenu = null;
        Unpause();
    }
}
