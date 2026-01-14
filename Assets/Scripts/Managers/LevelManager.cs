using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Danh sách level")]
    public List<LevelDataSO> allLevels;

    public LevelDataSO testLevel;

    private int currentLevelIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// get level data by index
    /// </summary>
    public LevelDataSO GetLevelData(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex > allLevels.Count)
        {
            Debug.LogError($"❌ Không tìm thấy Level {levelIndex}!");
            return null;
        }
        return allLevels[levelIndex - 1];
    }

    /// <summary>
    /// get current level from saved data
    /// </summary>
    public int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt("CurrentLevel", 0);
    }

    /// <summary>
    /// update level after completion
    /// </summary>
    public void CompleteLevel()
    {
        int nextLevel = GetCurrentLevel() + 1;
        PlayerPrefs.SetInt("CurrentLevel", nextLevel);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// check if current level is hard mode
    /// </summary>
    public bool IsHardMode()
    {
        return GetCurrentLevel() % 5 == 0;
    }
}