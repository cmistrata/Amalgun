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
    public GameObject WavesCounter;
    public GameObject GameOverScreen;
    public GameObject GameStartPrefab;
    public GameObject ExistingGame;
    public GameObject StartMenu;
    public bool Paused = false;
    public GameObject PauseOverlay;

    public static GameManager Instance;

    /// <summary>
    /// Object to be restarted to an initial state after each game over.
    /// </summary>
    public Transform GameObjectsContainer;

    /// <summary>
    /// Player prefab used to put in GameState after starting a new game.
    /// </summary>
    public GameObject PlayerPrefab;
    public GameObject RestarterPartPrefab;


    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space") && State == GameState.GameOver)
        {
            RestartGame();
        }

        if (Input.GetKeyDown("r")) {
            StartNewGaame();
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
    /// Start a new game.
    /// </summary>
    public void StartGame()
    {
        WavesCounter.SetActive(true);
        WavesManager.Instance.WaveIndex = -1;
        WavesManager.Instance.StartNextWave();
        MusicManager.Instance.RestartEasySong();
        State = GameState.Playing;
        //Player.Instance.Shooting = true;
    }

    /// <summary>
    /// Restart a game, with some minor differences to starting a game.
    /// </summary>
    public void RestartGame()
    {
        foreach (Transform transform in GameObjectsContainer)
        {
            Destroy(transform.gameObject);
        }
        GameOverScreen.SetActive(false);
        MusicManager.Instance.RestartEasySong();
        // Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity).transform.parent = GameObjectsContainer;
        Player.Instance.gameObject.SetActive(true);
        Player.Instance.GetComponent<Part>().Start();
        Instantiate(RestarterPartPrefab, new Vector3(5, 0, 0), Quaternion.identity).transform.parent = GameObjectsContainer;


        WavesManager.Instance.WaveIndex = -1;
        WavesManager.Instance.EnemyCount = 0;
        WavesManager.Instance.StartNextWave();
        State = GameState.Playing;
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


    public void StartNewGaame() {
        StartMenu.SetActive(false);
        Destroy(ExistingGame);
        ExistingGame = Instantiate(GameStartPrefab);
    }
}
