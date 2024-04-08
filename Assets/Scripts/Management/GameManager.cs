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

    public GameState State = GameState.Intro;
    public Player InitialPlayer;
    public Player Player;
    public GameObject Arena;
    
    public bool Paused = false;
    public GameObject PauseOverlay;

    public GameObject StartMenu;
    public GameObject GameOverScreen;
    public GameObject Shop;

    

    public int Money = 0;
    public TMP_Text MoneyDisplay;

    public int Wave = 1;
    public int EnemiesPerWave = 5;
    public TMP_Text WaveText;

    private int _enemiesRemaining = 0;


    void Awake()
    {
        Instance = this;
        Player.SignalPlayerDeath += HandlePlayerDeath;
        CellHealthManager.SignalEnemyDeath += HandleEnemyDeath;
    }

    private void Start() {
        if (State == GameState.Fighting) {
            StartNewGame();
        }
    }

    public void GainMoney(int amount = 1) {
        Money += amount;
        MoneyDisplay.text = $"Money :{Money}";
    }

    public void SpendMoney(int amount = 1) {
        Money -= amount;
        MoneyDisplay.text = $"Money :{Money}";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) {
            StartNewGame();
        }
        if (State == GameState.Fighting) {
            FightingUpdate();
        } else if (State == GameState.GameOver) {
            GameOverUpdate();
        }
    }

    void FightingUpdate() {
        if (Input.GetKeyDown(KeyCode.P)) {
            Paused = !Paused;
            if (Paused) {
                Time.timeScale = 0f;
                PauseOverlay.SetActive(true);
            } else {
                Time.timeScale = 1f;
                PauseOverlay.SetActive(false);
            }
        }

        if (Paused) return;

    }
    void GameOverUpdate() {
        if (Input.GetKeyDown(KeyCode.Space)) {
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
        MusicManager.Instance.RestartEasySong();
        Wave = 0;
        StartNewWave();
    }

    public void StartNewWave() {
        State = GameState.Fighting;

        Wave += 1;
        WaveText.text = $"Wave {Wave}";
        WaveText.gameObject.SetActive(true);
        _enemiesRemaining = EnemiesPerWave;
        EnemySpawner.Instance.SpawnMoreEnemies(_enemiesRemaining);
        
        
        Player.transform.position = Vector3.zero;
        Shop.SetActive(false);
        Arena.SetActive(true);
    }

    public void ClearUI() {
        StartMenu.SetActive(false);
        GameOverScreen.SetActive(false);
    }

    public void EndWave() {
        State = GameState.Shop;

        Arena.SetActive(false);
        WaveText.gameObject.SetActive(false);
        Shop.SetActive(true);
        Player.transform.position = Vector3.zero;
    }

    public void HandleShopContinue() {
        StartNewWave();
    }

    public void HandlePlayerDeath() {
        GameOver();
    }

    public void HandleEnemyDeath() {
        _enemiesRemaining -= 1;
        if (_enemiesRemaining <= 0 ) {
            EndWave();
        }
    }
}
