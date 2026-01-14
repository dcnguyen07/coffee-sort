using System;
using UnityEngine;

public class HeartManager : MonoBehaviour
{
    private const string HEART_KEY = "PlayerHeart";
    private const string TIMER_KEY = "HeartTimer";
    private const int MAX_HEART = 5;
    private const int HEART_RECOVERY_TIME = 30 * 60;

    public static HeartManager Instance;

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
    /// get current heart
    /// </summary>
    public int GetHeart()
    {
        return PlayerPrefs.GetInt(HEART_KEY, MAX_HEART);
    }

    /// <summary>
    /// update heart
    /// </summary>
    public void SetHeart(int amount)
    {
        amount = Mathf.Clamp(amount, 0, MAX_HEART);
        PlayerPrefs.SetInt(HEART_KEY, amount);
        PlayerPrefs.Save();

        if (amount == MAX_HEART)
        {
            PlayerPrefs.DeleteKey(TIMER_KEY);
        }

        GameManager.Instance.uiManager.barHeartManager.UpdateHeartUI();
    }

    /// <summary>
    /// check have heart timer
    /// </summary>
    public bool HasHeartTimer()
    {
        return PlayerPrefs.HasKey(TIMER_KEY);
    }

    /// <summary>
    /// get heart timer
    /// </summary>
    public int GetHeartTimer()
    {
        if (!HasHeartTimer()) return 0;

        string savedTime = PlayerPrefs.GetString(TIMER_KEY, "");

        if (string.IsNullOrEmpty(savedTime))
        {
            return 0;
        }

        long savedTimestamp;
        if (!long.TryParse(savedTime, out savedTimestamp))
        {
            return 0;
        }

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int remainingTime = (int)(HEART_RECOVERY_TIME - (currentTime - savedTimestamp));

        return Mathf.Max(0, remainingTime);
    }
    /// <summary>
    /// start heart timer
    /// </summary>
    public void StartHeartTimer()
    {
        if (!HasHeartTimer())
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PlayerPrefs.SetString(TIMER_KEY, currentTime.ToString());
            PlayerPrefs.Save();
        }

        GameManager.Instance.uiManager.barHeartManager.UpdateHeartUI();
    }

    /// <summary>
    /// add heart
    /// </summary>
    public void AddHeart()
    {
        int currentHeart = GetHeart();
        if (currentHeart < MAX_HEART)
        {
            SetHeart(currentHeart + 1);
            if (currentHeart + 1 == MAX_HEART)
            {
                PlayerPrefs.DeleteKey(TIMER_KEY);
            }
        }
    }

    /// <summary>
    /// spend heart
    /// </summary>
    public bool SpendHeart()
    {
        int currentHeart = GetHeart();
        if (currentHeart > 0)
        {
            currentHeart--;
            SetHeart(currentHeart);

            if (currentHeart < MAX_HEART && !HasHeartTimer())
            {
                StartHeartTimer();
                GameManager.Instance.uiManager.barHeartManager.UpdateHeartUI();
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Refill Heart
    /// </summary>
    public void RefillHeart()
    {
        SetHeart(MAX_HEART);
        PlayerPrefs.DeleteKey(TIMER_KEY);
    }
}