using System;
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
    public static GameManager Instance;

    public static Action SignalGameStart;

    public GameState State = GameState.Intro;
    public GameObject PlayerPrefab;
    public GameObject CurrentPlayer;
    public GameObject Arena;

    public bool Paused = false;
    public GameObject PauseOverlay;

    public GameObject StartMenu;
    public GameObject GameOverScreen;
    public GameObject Shop;



    public int Money = 0;
    public TMP_Text MoneyDisplay;

    public int Wave = 1;
    public TMP_Text WaveText;


    void Awake()
    {
        Instance = this;
        Player.SignalPlayerDeath += HandlePlayerDeath;
        Level.SignalLevelComplete += OnLevelComplete;
        WaveSpawner.SignalWaveComplete += OnNewWave;
        SignalGameStart += OnNewWave;
    }

    private void Start()
    {
        if (State == GameState.Fighting)
        {
            StartNewGame();
        }

    }

    public void GainMoney(int amount = 1)
    {
        Money += amount;
        MoneyDisplay.text = $"Money :{Money}";
    }

    public void SpendMoney(int amount = 1)
    {
        Money -= amount;
        MoneyDisplay.text = $"Money :{Money}";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartNewGame();
        }
        if (State == GameState.Fighting)
        {
            FightingUpdate();
        }
        else if (State == GameState.GameOver)
        {
            GameOverUpdate();
        }
    }

    void FightingUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Paused = !Paused;
            if (Paused)
            {
                Time.timeScale = 0f;
                PauseOverlay.SetActive(true);
            }
            else
            {
                Time.timeScale = 1f;
                PauseOverlay.SetActive(false);
            }
        }

        if (Paused) return;

    }
    void GameOverUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
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
        if (State == GameState.GameOver)
        {
            Debug.LogWarning("Tried to call GameOver while already in GameOver state.");
            return;
        }
        State = GameState.GameOver;

        Debug.Log("Game over!");
        MusicManager.Instance.StopMusic();
        State = GameState.GameOver;
    }

    //TODO refactor the rest of this into game start signal handling
    public void StartNewGame()
    {
        Debug.Log("Starting new game.");
        Money = 0;

        ClearUI();
        if (CurrentPlayer != null)
        {
            Destroy(CurrentPlayer);
        }
        CurrentPlayer = Instantiate(PlayerPrefab);
        CurrentPlayer.transform.position = Vector3.zero;
        State = GameState.Fighting;
        MusicManager.Instance.RestartEasySong();
        Wave = 0;

        SignalGameStart.Invoke();
    }

    public void OnNewWave()
    {
        State = GameState.Fighting;

        Wave += 1;
        WaveText.text = $"Wave {Wave}";
        WaveText.gameObject.SetActive(true);


        CurrentPlayer.transform.position = Vector3.zero;
        Shop.SetActive(false);
        Arena.SetActive(true);
    }

    public void ClearUI()
    {
        StartMenu.SetActive(false);
        GameOverScreen.SetActive(false);
    }

    public void OnLevelComplete()
    {
        State = GameState.Shop;

        Arena.SetActive(false);
        WaveText.gameObject.SetActive(false);
        Shop.SetActive(true);
        CurrentPlayer.transform.position = Vector3.zero;
    }

    public void HandleShopContinue()
    {
        OnNewWave();
    }

    public void HandlePlayerDeath()
    {
        GameOver();
    }
}
