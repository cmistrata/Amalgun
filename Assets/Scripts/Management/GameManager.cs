using System;
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
    private const float _initialEnemySpawnInterval = .2f;
    // How many enemies we spawn in the initial spawn burst, as well
    // as a floor that we will spawn an enemy upon dropping below (if any are left).
    private const int _minEnemies = 3;
    private const float _laterEnemySpawnInterval = 2f;
    private Dictionary<GameState, Tuple<Action, Action>> EntryAndExitActionByState;


    [Header("General State")]
    public bool Paused = false;
    public GameObject Player;
    public GameState State;

    [Header("Fighting State")]
    public LevelList Levels;
    public bool SpawnEnemies = true;
    public bool AutoKoEnemies;

    public int Level0Indexed = 0;
    public int Wave0Indexed = 0;
    private HashSet<GameObject> _activeEnemies = new();
    // private int _activeEnemies = 0;
    private List<List<CellType>> _wavesToSpawn = new();
    private List<CellType> _enemyTypesToSpawn = new();
    private float _enemySpawnTimer = 0;
    private const int _wavesPerLevel = 3;
    private float _enemiesSpawnedInWave;

    [Header("Shopping State")]
    public Shop Shop;
    public int Money = 0;
    public int MeldCost = 2;
    [HideInInspector]
    public ShopItem SelectedShopItem;


    [Header("UI and Menus")]
    public GameObject PauseOverlay;
    public GameObject GameOverScreen;

    private Dictionary<Action, Action> _signalHandlers = new Dictionary<Action, Action>();


    void Awake() {
        if (Instance != null) Debug.LogError("Multiple game managers instantiated");
        Instance = this;
        global::Player.SignalPlayerDeath += HandlePlayerDeath;
        CellHealthManager.SignalEnemyCellDefeat += HandleEnemyCellDefeat;
        ShopItem.SignalShopItemBought += HandleShopItemBought;
        ShopItem.SignalShopItemSelected += HandleShopItemSelected;
        // EntryAndExitActionByState = new Dictionary<GameState, Tuple<Action, Action>>() {
        //     [GameState.Fighting] = (EnterFightingState, ExitFightingState)
        // };
    }

    private void OnDestroy() {
        global::Player.SignalPlayerDeath -= HandlePlayerDeath;
        CellHealthManager.SignalEnemyCellDefeat -= HandleEnemyCellDefeat;
        ShopItem.SignalShopItemBought -= HandleShopItemBought;
        ShopItem.SignalShopItemSelected -= HandleShopItemSelected;
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
        Shop.Disappear();
    }

    void LoadLevel(int newLevelNumber) {
        Level0Indexed = newLevelNumber;
        var difficulty = CalculateLevelDifficulty(Level0Indexed);
        for (int i = 0; i < _wavesPerLevel; i++) {
            _wavesToSpawn.Add(GenerateWave(difficulty));
        }
        _enemyTypesToSpawn = new();
        _enemySpawnTimer = _initialEnemySpawnInterval;
    }

    bool NoEnemiesLeftInLevel() {
        return _wavesToSpawn.Count == 0 && _enemyTypesToSpawn.Count == 0 && _activeEnemies.Count() == 0;
    }

    void FightingUpdate() {
        if (NoEnemiesLeftInLevel()) {
            AudioManager.Instance.PlayNewWaveSound();
            EnterShopState();
            return;
        }
        else if (SpawnEnemies && !_enemyTypesToSpawn.Any() && _activeEnemies.Count() == 0 && _wavesToSpawn.Any()) {
            QueueNewWave();
        }
        else if (_enemyTypesToSpawn.Any()) {
            _enemySpawnTimer -= Time.deltaTime;
            // Lower the timer quickly if we haven't spawned the minimum yet.
            if (_enemiesSpawnedInWave < Math.Min(Level0Indexed + _minEnemies, 6)) {
                _enemySpawnTimer = Math.Min(_enemySpawnTimer, _initialEnemySpawnInterval);
            }
            if (_enemySpawnTimer <= 0) {
                var _enemyTypeToSpawn = _enemyTypesToSpawn[0];
                _enemyTypesToSpawn.RemoveAt(0);
                var newEnemy = EnemySpawner.SpawnEnemy(_enemyTypeToSpawn);
                _activeEnemies.Add(newEnemy);
                _enemySpawnTimer += _laterEnemySpawnInterval;
                _enemiesSpawnedInWave += 1;
            }
        }
    }

    void QueueNewWave() {
        Wave0Indexed += 1;
        if (Wave0Indexed > 0) {
            AudioManager.Instance.PlayNewWaveSound();
        }
        _enemyTypesToSpawn.AddRange(_wavesToSpawn[0]);
        _wavesToSpawn.RemoveAt(0);
        _enemiesSpawnedInWave = 0;
    }

    public void ForceEndLevel() {
        _wavesToSpawn.Clear();
        _enemyTypesToSpawn.Clear();
        int safetyCheck = 0;
        while (_activeEnemies.Count() > 0 && safetyCheck < 100) {
            safetyCheck += 1;
            _activeEnemies.First().GetComponent<CellHealthManager>().Defeat();
        }
        // foreach (GameObject enemy in _activeEnemies) {
        //     if (enemy == null) {
        //         Debug.LogError("Found null enemy when force ending level.");
        //         continue;
        //     }
        //     enemy.GetComponent<CellHealthManager>().Defeat();
        // }
    }

    void HandleCellClick() {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!Utils.MouseRaycast(out RaycastHit hit, Layers.PlayerCell)) return;

        if (hit.collider.gameObject.TryGetComponent<Cell>(out var cell)) {
            if (cell.gameObject == Player) return;
            if (cell.State == CellState.Friendly && SelectedShopItem != null && Money >= SelectedShopItem.Cost) {
                Money -= SelectedShopItem.Cost;
                if (SelectedShopItem.Type == ShopItemType.Melder) {
                    AudioManager.Instance.PlayAttachSound();
                    cell.ChangeState(CellState.Melded);
                }
                PurchaseSelectedItem();
            }
        }
    }

    void PurchaseSelectedItem() {
        Money -= SelectedShopItem.Cost;
        Destroy(SelectedShopItem.gameObject);
        SelectedShopItem = null;
    }

    void ShopUpdate() {
        HandleCellClick();
        if (Input.GetKeyDown(KeyCode.Space)) {
            Level0Indexed += 1;
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
        Shop.Appear();
        State = GameState.Shop;
    }

    public void EnterFightingState() {
        DisableStatefulObjects();
        Wave0Indexed = -1;
        LoadLevel(Level0Indexed);
        State = GameState.Fighting;
    }

    public void ExitFightingState() {

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
        Level0Indexed = 0;
        GameOverScreen.SetActive(false);
        DestroyGameElements();

        Player = Instantiate(Globals.Instance.PlayerPrefab);
        Player.name = "Player";
        Player.transform.position = Vector3.zero;
        MusicManager.Instance.RestartEasySong();

        EnterFightingState();
    }


    private static int CalculateLevelDifficulty(int levelNumber0Indexed) {
        return levelNumber0Indexed * 3 + 5;
    }

    private static List<CellType> GenerateWave(int difficulty) {
        int difficultyRemaining = difficulty;
        List<CellType> wave = new();
        int safetyCounter = 0;
        while (safetyCounter < 1000 && difficultyRemaining > 0) {
            var potentialNextCell = Globals.StatsByCellType.ChooseRandomWeighted(
                cellTypeAndStats => 1f / cellTypeAndStats.Value.Rarity
            );
            if (potentialNextCell.Value.Difficulty <= (difficultyRemaining + 1)) {
                wave.Add(potentialNextCell.Key);
                difficultyRemaining -= potentialNextCell.Value.Difficulty;
            }
            safetyCounter += 1;
        }
        if (safetyCounter >= 1000) {
            Debug.LogError("Error in while loop while generating wave.");
        }
        return wave;
    }

    #region Signal Handlers
    public void HandleShopContinue() {
        Level0Indexed += 1;
        LoadLevel(Level0Indexed);
        EnterFightingState();
    }

    public void HandlePlayerDeath() {
        EnterGameOverState();
    }

    public void HandleEnemyCellDefeat(GameObject enemyCell) {
        if (!_activeEnemies.Remove(enemyCell)) {
            Debug.LogWarning($"Enemy cell {enemyCell} was defeated but wasn't being tracked by game manager.");
        }
        Money += 1;
    }

    void HandleShopItemBought(ShopItem boughtItem) {
        return;
    }

    void HandleShopItemSelected(ShopItem selectedItem) {
        Debug.Log($"Selecting {selectedItem.gameObject}");
        SelectedShopItem = selectedItem;
    }
    #endregion



    #region Getters
    public static Vector3 GetPlayerPosition() {
        if (Instance == null || Instance.Player == null) return Vector3.zero;
        return Instance.Player.transform.position;
    }

    public static GameObject GetPlayer() {
        if (Instance == null) return null;
        return Instance.Player;
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

    public static int GetMaxDashes() {
        if (Instance == null) return -1;
        if (Instance.Player == null) return -1;
        return Instance.Player.GetComponent<Player>().MaxDashes;
    }

    public static int GetCurrentDashes() {
        if (Instance == null) return -1;
        if (Instance.Player == null) return -1;
        return (int)Mathf.Floor(Instance.Player.GetComponent<Player>().CurrentDashCharge);
    }

    public static int GetMoney() {
        if (Instance == null) return -1;
        return Instance.Money;
    }

    public static int EnemiesLeftInWave() {
        if (Instance == null) return -1;
        return Instance._activeEnemies.Count();
    }
    #endregion
}
