using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum GameState
{
    Intro,
    Playing,
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
    public CameraManager CameraManager;

    public static GameManager Instance;


    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r")) {
            StartNewGame();
        }
        if (State == GameState.Playing) {
            PlayingUpdate();
        }
    }

    void PlayingUpdate() {
        if (Input.GetKeyDown("space") && State == GameState.GameOver) {
            StartNewGame();
        }
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

    /// <summary>
    /// End the game, bringing up the game over screen.
    /// </summary>
    public void GameOver()
    {
        if (State == GameState.GameOver)
        {
            Debug.LogWarning("Tried to call GameOver while already in GameOver state.");
            return;
        }

        Debug.Log("Game over!");
        GameOverScreen.SetActive(true);
        MusicManager.Instance.StopMusic();
        // AudioManager.Instance.PlayGameOver();
        State = GameState.GameOver;
    }

    void OnKeyDown(KeyDownEvent ev) {
        Debug.Log("KeyDown:" + ev.keyCode);
        Debug.Log("KeyDown:" + ev.character);
        Debug.Log("KeyDown:" + ev.modifiers);
    }


    public void StartNewGame() {
        StartMenu.SetActive(false);
        if (Arena != null) {
            Destroy(Arena.gameObject);
        }
        Arena = Instantiate(InitialArena);
        Arena.EnemySpawnerAndCounter.SignalWaveOver += HandleWaveOver;
        if (Player != null) {
            Destroy(Player.gameObject);
        }
        Player = Instantiate(InitialPlayer);
        CameraManager.Focus = Player.transform;
    }

    public void HandleWaveOver() {
        Debug.Log("Hello!");
    }
}
