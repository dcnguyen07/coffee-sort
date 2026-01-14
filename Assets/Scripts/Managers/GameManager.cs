using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public enum GameState
{
    InHome,
    InPause,
    Preparing,
    Playing,
    OutOfSpace,
    GameOver,
    LevelComplete
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configs")]
    public TrayConfigSO trayConfigSO;
    public GameConfigSO gameConfigSO;

    [Header("Managers")]
    public UIManager uiManager;
    public BoardManager boardManager;
    public LevelManager levelManager;

    [Header("Game Economy")]
    private const string COIN_KEY = "player_coins";
    public int CurrentCoins { get; private set; }
    public List<UICoinDisplay> coinDisplays = new List<UICoinDisplay>();

    [Header("Booster Count")]
    public int moreSlotsCount;
    public int orderCount;
    public int undoCount;

    public GameState currentState;
    private int unlockedSlots = 4;

    private const string HEART_KEY = "player_hearts";
    private const string HEART_TIMER_KEY = "heart_timer";
    private const int MAX_HEARTS = 5;
    private const int HEART_RECOVERY_TIME = 1800;
    private Coroutine heartRecoveryCoroutine;
    public bool isTestMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
    }
    private void Start()
    {
       LoadGame();
    }
    public int GetCurrentLevel()
    {
        if (PlayerPrefs.HasKey("CurrentLevel"))
        {
            return PlayerPrefs.GetInt("CurrentLevel", 1); 
        }
        return 1;
    }
    public void SaveCurrentLevel(int currentLevel)
    {
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
    }

    public int GetCurrentHardLevel()
    {
        if (PlayerPrefs.HasKey("PlayerHardLevel"))
        {
            return PlayerPrefs.GetInt("PlayerHardLevel", 1);
        }

        return 1; 
    }
    public bool IsHardMode()
    {
        int currentLevel = GetCurrentLevel();
        int currentHardLevel = GetCurrentHardLevel();

        return false;
    }

    public void LoadGame()
    {
        unlockedSlots = 4;
        currentState = GameState.Preparing;

        uiManager.HideAllPanels();
        uiManager.uiGame.InitUI();
        uiManager.uiGame.Show();

        int levelIndex = GetCurrentLevel();
        LevelDataSO levelData = levelManager.GetLevelData(levelIndex);

        if (levelData == null)
        {
            Debug.LogError($"❌ Không tìm thấy dữ liệu level {levelIndex}!");
            return;
        }

        if (!isTestMode)
            boardManager.SetLevelData(levelData);
        else
            boardManager.SetLevelData(LevelManager.Instance.testLevel);
        boardManager.GenerateLevel();

        uiManager.uiGame.UpdateUI();

        if (IsHardMode())
        {
            
        }

    }
    

    /// <summary>
    /// Unlock new slot on 
    /// </summary>
    public void UnlockSlot()
    {
        if (unlockedSlots < 7)
        {
            unlockedSlots++;
            boardManager.UnlockNewSlot();
            currentState = GameState.Playing;
        }
        else
        {
            currentState = GameState.GameOver;
            uiManager.uiGameFail.Show();
        }
    }

    /// <summary>
    /// Restart level when fail
    /// </summary>
    public void RestartLevel()
    {
        boardManager.ClearBoard();
        LoadGame();
    }

    /// <summary>
    /// update game state
    /// </summary>
    public void SetGameState(GameState newState)
    {
        currentState = newState;
    }

    /// <summary>
    /// get current game state
    /// </summary>
    public GameState GetGameState()
    {
        return currentState;
    }

    public void LoadNextLevel()
    {
        boardManager.ClearBoard();
        int nextLevel = GetCurrentLevel() + 1;
        SaveCurrentLevel(nextLevel);
        LoadGame();
    }

    /// <summary>
    /// load coin 
    /// </summary>
    private void LoadCoin()
    {
        CurrentCoins = PlayerPrefs.GetInt(COIN_KEY, 0);
        UpdateAllCoinDisplays();
    }

    private void LoadBoosterCount()
    {
        moreSlotsCount = PlayerPrefs.GetInt("MoreSlots", 3);
        //Debug.Log("SLOTS : " + moreSlotsCount);
        orderCount = PlayerPrefs.GetInt("Order", 1);
        //Debug.Log("ORDER : " + orderCount);
        undoCount = PlayerPrefs.GetInt("Undo", 1);
        //Debug.Log("UNDO : " + undoCount);
    }

    /// <summary>
    /// save coin
    /// </summary>
    private void SaveCoin()
    {
        PlayerPrefs.SetInt(COIN_KEY, CurrentCoins);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// check have enough coin
    /// </summary>
    public bool HasEnoughCoins(int amount)
    {
        return CurrentCoins >= amount;
    }

    /// <summary>
    /// add coin
    /// </summary>
    public void AddCoins(int amount)
    {
        CurrentCoins += amount;
        SaveCoin();
        UpdateAllCoinDisplays();
    }

    /// <summary>
    /// spend coin
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (HasEnoughCoins(amount))
        {
            CurrentCoins -= amount;
            SaveCoin();
            UpdateAllCoinDisplays();
            return true;
        }
        return false;
    }

    public void SpendBooster(int amount, BoosterButton.BoosterType type)
    {
        if (type == BoosterButton.BoosterType.Slot)
        {
            if (moreSlotsCount - amount >= 0)
            {
                moreSlotsCount -= amount;
                PlayerPrefs.SetInt("MoreSlots", moreSlotsCount);
            }
        }

        else if (type == BoosterButton.BoosterType.Order)
        {
            if (orderCount - amount >= 0)
            {
                orderCount -= amount;
                PlayerPrefs.SetInt("Order", orderCount);
            }
        }

        else if (type == BoosterButton.BoosterType.Undo)
        {
            if (undoCount - amount >= 0)
            {
                undoCount -= amount;
                PlayerPrefs.SetInt("Undo", undoCount);
            }
        }
    }

    public void MoreBooster(int amount, BoosterButton.BoosterType type)
    {
        if (type == BoosterButton.BoosterType.Slot)
        {

            moreSlotsCount += amount;
            PlayerPrefs.SetInt("MoreSlots", moreSlotsCount);
            GameManager.Instance.uiManager.uiGame.btnAddSlot.UpdateBoosterState();

        }

        else if (type == BoosterButton.BoosterType.Order)
        {

            orderCount += amount;
            PlayerPrefs.SetInt("Order", orderCount);
            GameManager.Instance.uiManager.uiGame.btnOrder.UpdateBoosterState();
        }

        else if (type == BoosterButton.BoosterType.Undo)
        {

            undoCount += amount;
            PlayerPrefs.SetInt("Undo", undoCount);
            GameManager.Instance.uiManager.uiGame.btnUndo.UpdateBoosterState();
        }
    }

    /// <summary>
    /// update coin display
    /// </summary>
    public void RegisterCoinDisplay(UICoinDisplay display)
    {
        if (!coinDisplays.Contains(display))
        {
            coinDisplays.Add(display);
        }
        display.UpdateCoinText(CurrentCoins);
    }

    /// <summary>
    /// update all coin display
    /// </summary>
    private void UpdateAllCoinDisplays()
    {
        foreach (var display in coinDisplays)
        {
            display.UpdateCoinText(CurrentCoins);
        }
    }

    /// <summary>
    /// Unlock one locked slot on the board
    /// </summary>
    public void AddSlot()
    {

        if (boardManager == null) return;

        BoardSlot lockedSlot = boardManager.GetFirstLockedSlot();
        if (lockedSlot != null)
        {

            int remainingSlots = boardManager.GetRemainingLockedSlots();
            lockedSlot.UnlockSlotAds();

            uiManager.uiGame.UpdateSlotDisplay(remainingSlots);
            SaveUnlockedSlots();
        }
        else
        {
            Debug.Log("Không còn slot nào để mở khóa! / No locked slots remaining!");
        }
    }

    /// <summary>
    /// Save unlocked slots state
    /// </summary>
    private void SaveUnlockedSlots()
    {
        int unlockedSlots = boardManager.GetUnlockedSlotCount();
        PlayerPrefs.SetInt("UnlockedSlots", unlockedSlots);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Shuffle cups in queue while keeping color counts unchanged
    /// </summary>
    public void SortTrays()
    {
       

        List<CupModel> cupQueue = boardManager.cupQueue;
        if (cupQueue.Count < 2) return;

        Dictionary<int, int> colorCount = new Dictionary<int, int>();
        foreach (var cup in cupQueue)
        {
            if (colorCount.ContainsKey(cup.colorIndex))
                colorCount[cup.colorIndex]++;
            else
                colorCount[cup.colorIndex] = 1;
        }

        List<int> shuffledColors = new List<int>();
        foreach (var pair in colorCount)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                shuffledColors.Add(pair.Key);
            }
        }

        System.Random rng = new System.Random();
        shuffledColors = shuffledColors.OrderBy(_ => rng.Next()).ToList();

        for (int i = 0; i < cupQueue.Count; i++)
        {
            cupQueue[i].SetCupColor(shuffledColors[i]);
        }

        Debug.Log("Queue shuffled!");
    }

    private Stack<UndoAction> undoStack = new Stack<UndoAction>();

    /// <summary>
    /// add action to stack
    /// </summary>
    public void SaveActionForUndo(UndoAction action)
    {
        undoStack.Push(action);
    }

    /// <summary>
    /// check can undo
    /// </summary>
    public bool CanUndo()
    {
        return undoStack.Count > 0 && boardManager.IsQueueIdle();
    }

    public void OnUndo()
    {
        if (undoStack.Count == 0 || !boardManager.IsQueueIdle())
        {
            return;
        }

        UndoAction lastAction = undoStack.Pop();

        if (lastAction.tray != null && lastAction.tray.IsOnBoard())
        {
            //Debug.Log("MOVE TRAY");
            lastAction.tray.ReleaseCups();
            boardManager.UpdateCupLeftDisplay();
            boardManager.ClearTrayFromBoard(lastAction.tray);
            boardManager.UndoTrayMove(lastAction.tray, lastAction.previousSlot);
        }
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleSound();
    }

    public void ToggleMusic()
    {
        SoundManager.Instance.ToggleMusic();
    }

    public void ToggleShock()
    {
        SoundManager.Instance.ToggleShock();
    }

    public bool IsSoundOn() => PlayerPrefs.GetInt("Sound", 1) == 1;
    public bool IsMusicOn() => PlayerPrefs.GetInt("Music", 1) == 1;
    public bool IsShockOn() => PlayerPrefs.GetInt("Shock", 1) == 1;

    public void ReturnToHome()
    {
        boardManager.ClearBoard();
        uiManager.uiGame.Hide();
        currentState = GameState.InHome;
    }

    public int GetCoin()
    {
        return PlayerPrefs.GetInt(COIN_KEY, 0);
    }

    public int GetBoosterCount(BoosterButton.BoosterType currentType)
    {
        if (currentType == BoosterButton.BoosterType.Slot)
        {
            return moreSlotsCount;
        }
        else if (currentType == BoosterButton.BoosterType.Order)
        {
            return orderCount;
        }
        else if (currentType == BoosterButton.BoosterType.Undo)
        {
            return undoCount;
        }
        return 0;
    }
}