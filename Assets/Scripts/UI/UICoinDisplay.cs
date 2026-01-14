using UnityEngine;
using UnityEngine.UI;

public class UICoinDisplay : MonoBehaviour
{
    public Text coinText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCoinDisplay(this);
        }
    }

    /// <summary>
    /// Update coin text on UI
    /// </summary>
    public void UpdateCoinText(int amount)
    {
        coinText.text = amount.ToString();
    }
}