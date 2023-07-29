using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
