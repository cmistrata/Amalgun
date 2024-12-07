using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum GameState {
    Fighting,
    Shop,
    GameOver,
}

[System.Serializable]
public class Level {
    public List<Wave> Waves;
}

[System.Serializable]
public class LevelList {
    public List<Level> Levels;
}



public class GameManager : MonoBehaviour {
    public static GameManager Instance;
    private const float _enemySpawnInterval = .2f;

    public LevelList Levels;
    public bool SpawnEnemies = true;


    [Header("Game State")]
    public GameState State;
    public int Money = 0;

    public int LevelNumber = 0;
    private int _activeEnemies = 0;
    private List<List<CellType>> _wavesToSpawn = new();
    private List<CellType> _enemyTypesToSpawn = new();
    private float _enemySpawnTimer = 0;

    public bool Paused = false;
    public GameObject Player;
    public GameObject Shop;


    [Header("UI and Menus")]
    public GameObject PauseOverlay;
    public GameObject GameOverScreen;
    public TMP_Text MoneyDisplay;


    void Awake() {
        if (Instance != null) Debug.LogError("Multiple game managers instantiated");
        Instance = this;
        global::Player.SignalPlayerDeath += HandlePlayerDeath;
        CellHealthManager.SignalEnemyCellDefeat += HandleEnemyCellDefeat;
    }

    private void OnDestroy() {
        global::Player.SignalPlayerDeath -= HandlePlayerDeath;
        CellHealthManager.SignalEnemyCellDefeat -= HandleEnemyCellDefeat;
    }

    void Start() {
        StartNewGame();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            StartNewGame();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        if (State == GameState.Fighting || State == GameState.Shop) {
            CheckForPause();
        }

        if (State == GameState.Fighting) {
            FightingUpdate();
        }
        else if (State == GameState.GameOver) {
            GameOverUpdate();
        }
        else if (State == GameState.Shop) {
            ShopUpdate();
        }
    }

    void CheckForPause() {
        if (Input.GetKeyDown(KeyCode.P)) {
            Paused = !Paused;
            if (Paused) {
                Time.timeScale = 0f;
                PauseOverlay.SetActive(true);
            }
            else {
                Time.timeScale = 1f;
                PauseOverlay.SetActive(false);
            }
        }
    }

    void DisableStatefulObjects() {
        GameOverScreen.SetActive(false);
        Shop.SetActive(false);
    }

    void LoadLevel(int newLevelNumber) {
        LevelNumber = newLevelNumber;
        Level newLevel = LevelNumber < Levels.Levels.Count - 1 ? Levels.Levels[LevelNumber] : Levels.Levels[^1];
        _wavesToSpawn = newLevel.Waves
            .Select(wave => wave.Enemies.ToList())
            .ToList();
        _enemyTypesToSpawn = new();
        _activeEnemies = 0;
        _enemySpawnTimer = _enemySpawnInterval;
    }

    bool NoEnemiesLeftInLevel() {
        return _wavesToSpawn.Count == 0 && _enemyTypesToSpawn.Count == 0 && _activeEnemies == 0;
    }

    void FightingUpdate() {
        if (NoEnemiesLeftInLevel()) {
            AudioManager.Instance.PlayNewWaveSound();
            EnterShopState();
            return;
        }
        if (!_enemyTypesToSpawn.Any() && _activeEnemies <= 1 && _wavesToSpawn.Any() && SpawnEnemies) {
            _enemyTypesToSpawn.AddRange(_wavesToSpawn[0]);
            _wavesToSpawn.RemoveAt(0);
        }
        if (_enemyTypesToSpawn.Any()) {
            _enemySpawnTimer -= Time.deltaTime;
            if (_enemySpawnTimer <= 0) {
                var _enemyTypeToSpawn = _enemyTypesToSpawn[0];
                _enemyTypesToSpawn.RemoveAt(0);
                EnemySpawner.SpawnEnemy(_enemyTypeToSpawn);
                _activeEnemies++;
                _enemySpawnTimer += _enemySpawnInterval;
            }
        }
    }

    void ShopUpdate() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            LevelNumber += 1;
            EnterFightingState();
        }
    }

    void GameOverUpdate() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            StartNewGame();
        }
    }

    public void EnterGameOverState() {
        DisableStatefulObjects();
        GameOverScreen.SetActive(true);
        State = GameState.GameOver;
        Debug.Log("Game over!");
        MusicManager.Instance.StopMusic();
    }

    public void EnterShopState() {
        DisableStatefulObjects();
        foreach (Transform bullet in Containers.Bullets) {
            Destroy(bullet.gameObject);
        }
        Shop.SetActive(true);
        State = GameState.Shop;
    }

    public void EnterFightingState() {
        DisableStatefulObjects();
        LoadLevel(LevelNumber);
        State = GameState.Fighting;
    }

    void DestroyGameElements() {
        if (Player != null) {
            Destroy(Player);
        }
        foreach (Transform cell in Containers.Cells) {
            Destroy(cell.gameObject);
        }
        foreach (Transform bullet in Containers.Bullets) {
            Destroy(bullet.gameObject);
        }
        foreach (Transform effect in Containers.Effects) {
            Destroy(effect.gameObject);
        }
    }

    public void StartNewGame() {
        Debug.Log("Starting new game.");

        // Reset state.
        Money = 0;
        LevelNumber = 0;
        GameOverScreen.SetActive(false);
        DestroyGameElements();

        Player = Instantiate(Globals.Instance.PlayerPrefab);
        Player.name = "Player";
        Player.transform.position = Vector3.zero;
        MusicManager.Instance.RestartEasySong();

        EnterFightingState();
    }

    public static Vector3 GetPlayerPosition() {
        if (Instance == null || Instance.Player == null) return Vector3.zero;
        return Instance.Player.transform.position;
    }

    public void HandleShopContinue() {
        LevelNumber += 1;
        LoadLevel(LevelNumber);
        EnterFightingState();
    }

    public void HandlePlayerDeath() {
        EnterGameOverState();
    }

    public void HandleEnemyCellDefeat() {
        _activeEnemies -= 1;
    }

    public static int GetMaxHealth() {
        if (Instance == null) return -1;
        if (Instance.Player == null) return -1;
        return Instance.Player.GetComponent<Player>().MaxHealth;
    }

    public static int GetHealth() {
        if (Instance == null) return -1;
        if (Instance.Player == null) return -1;
        return Instance.Player.GetComponent<Player>().Health;
    }
}
