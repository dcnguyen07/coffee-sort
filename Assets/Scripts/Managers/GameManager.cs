using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Objects;

namespace Managers
{
    public enum GameState
    {
        InHome,
        InPause,
        Preparing,
        Playing,
        GameOver,
        LevelComplete
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager instance { get; private set; }

        [FormerlySerializedAs("trayConfigSO")] [Header("Configs")]
        public TrayConfigSO trayConfigSo;
        [FormerlySerializedAs("gameConfigSO")] public GameConfigSO gameConfigSo;

        [Header("Managers")]
        public UIManager uiManager;
        public BoardManager boardManager;
        public LevelManager levelManager;


        [Header("Booster Count")]
        public int moreSlotsCount;
        public int orderCount;
        public int undoCount;

        public GameState currentState;
        private int unlockedSlots = 4;

        public bool isTestMode;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
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
            GetCurrentLevel();
            GetCurrentHardLevel();

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
                return;
            }

            if (!isTestMode)
                boardManager.SetLevelData(levelData);
            else
                boardManager.SetLevelData(LevelManager.Instance.testLevel);
            boardManager.GenerateLevel();
            uiManager.uiGame.UpdateUI();
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
                GameManager.instance.uiManager.uiGame.btnAddSlot.UpdateBoosterState();
            }

            else if (type == BoosterButton.BoosterType.Order)
            {
                orderCount += amount;
                PlayerPrefs.SetInt("Order", orderCount);
                GameManager.instance.uiManager.uiGame.btnOrder.UpdateBoosterState();
            }

            else if (type == BoosterButton.BoosterType.Undo)
            {
                undoCount += amount;
                PlayerPrefs.SetInt("Undo", undoCount);
                GameManager.instance.uiManager.uiGame.btnUndo.UpdateBoosterState();
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
            int unlockedSlotCount = boardManager.GetUnlockedSlotCount();
            PlayerPrefs.SetInt("UnlockedSlots", unlockedSlotCount);
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
}