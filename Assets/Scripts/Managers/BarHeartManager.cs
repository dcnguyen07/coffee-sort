using UnityEngine;
using UnityEngine.UI;

public class BarHeartManager : MonoBehaviour
{
    [SerializeField] private Text heartValueText;
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject fullText;

    private void Start()
    {
        UpdateHeartUI();
        InvokeRepeating(nameof(UpdateTimer), 1f, 1f);
    }

    /// <summary>
    /// Update UI Heart on status bar
    /// </summary>
    public void UpdateHeartUI()
    {
        int currentHeart = HeartManager.Instance.GetHeart();
        heartValueText.text = currentHeart.ToString();

        if (currentHeart >= 5)
        {
            fullText.SetActive(true);
            timerObject.SetActive(false);
        }
        else
        {
            fullText.SetActive(false);
            timerObject.SetActive(HeartManager.Instance.HasHeartTimer());

            if (timerObject.activeSelf)
            {
                UpdateTimer();
            }
        }
    }

    private void UpdateTimer()
    {
        int remainingTime = HeartManager.Instance.GetHeartTimer();
        timerText.text = FormatTime(remainingTime);

        if (remainingTime <= 0)
        {
            HeartManager.Instance.AddHeart();
            UpdateHeartUI();
        }
    }

    private string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int sec = seconds % 60;
        return $"{minutes:D2}:{sec:D2}";
    }
}