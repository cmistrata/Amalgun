using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public enum GameState
{
    Intro,
    Fighting,
    Shop,
    GameOver,
}
public class GameManager : MonoBehaviour
{
    public GameState State = GameState.Intro;
    public Player InitialPlayer;
    private Player Player;
    public Arena InitialArena;
    private Arena Arena;
    
    public bool Paused = false;
    public GameObject PauseOverlay;

    public GameObject StartMenu;
    public GameObject GameOverScreen;
    public GameObject Shop;
    public CameraManager CameraManager;

    public static GameManager Instance;

    public int Money = 0;
    public TMP_Text moneyDisplay;


    void Awake()
    {
        Instance = this;
        EnemySpawnerAndCounter.SignalWaveOver += HandleWaveOver;
        Player2D.SignalPlayerDeath += HandlePlayerDeath;
    }

    public void GainMoney(int amount = 1) {
        Money += amount;
        moneyDisplay.text = $"Money :{Money}";
    }

    public void SpendMoney(int amount = 1) {
        Money -= amount;
        moneyDisplay.text = $"Money :{Money}";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r")) {
            StartNewGame();
        }
        if (State == GameState.Fighting) {
            PlayingUpdate();
        } else if (State == GameState.GameOver) {
            GameOverUpdate();
        }
    }

    void PlayingUpdate() {
        if (Input.GetKeyDown("p")) {
            Paused = !Paused;
            if (Paused) {
                Time.timeScale = 0f;
                PauseOverlay.SetActive(true);
            } else {
                Time.timeScale = 1f;
                PauseOverlay.SetActive(false);
            }
        }
    }
    void GameOverUpdate() {
        if (Input.GetKeyDown("space")) {
            StartNewGame();
        }
    }

    /// <summary>
    /// End the game, bringing up the game over screen.
    /// </summary>
    public void GameOver()
    {
        ClearUI();
        GameOverScreen.SetActive(true);
        if (State == GameState.GameOver) {
            Debug.LogWarning("Tried to call GameOver while already in GameOver state.");
            return;
        }
        State = GameState.GameOver;

        Debug.Log("Game over!");
        MusicManager.Instance.StopMusic();
        State = GameState.GameOver;
    }

    void OnKeyDown(KeyDownEvent ev) {
        Debug.Log("KeyDown:" + ev.keyCode);
        Debug.Log("KeyDown:" + ev.character);
        Debug.Log("KeyDown:" + ev.modifiers);
    }


    public void StartNewGame() {
        Debug.Log("Starting new game.");
        Money = 0;

        ClearUI();
        if (Player != null) {
            Destroy(Player.gameObject);
        }
        Player = Instantiate(InitialPlayer);
        Player.transform.position = Vector3.zero;
        State = GameState.Fighting;
        CameraManager.Focus = Player.transform;
        MusicManager.Instance.RestartEasySong();
        StartNewWave();
    }

    public void StartNewWave() {
        State = GameState.Fighting;

        Shop.SetActive(false);
        if (Arena != null) {
            Destroy(Arena.gameObject);
        }
        Arena = Instantiate(InitialArena);
        Player.transform.position = Vector3.zero;
    }

    public void HandleWaveOver() {
        State = GameState.Shop;

        Arena.gameObject.SetActive(false);
        Shop.SetActive(true);
        Player.transform.position = Vector3.zero;
    }

    public void HandleShopContinue() {
        StartNewWave();
    }

    public void HandlePlayerDeath() {
        GameOver();
    }

    public void ClearUI() {
        StartMenu.SetActive(false);
        GameOverScreen.SetActive(false);
    }
}
