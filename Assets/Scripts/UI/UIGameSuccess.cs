using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIGameSuccess : UIDialog
{
    [SerializeField] private Text uiLevel;
    [SerializeField] private Text uiTextReward;
    [SerializeField] private Button btnContinue;
    [SerializeField] private int rewardCoins = 90;

    public void InitUI()
    {
        uiLevel.text = "Level " + GameManager.Instance.GetCurrentLevel();
        uiTextReward.text = "+" + rewardCoins.ToString();
    }

    private void Start()
    {
        btnContinue.onClick.AddListener(OnContinueClicked);
    }

    public void ShowSuccess(int level)
    {
        uiLevel.text = "Level " + level;
        uiTextReward.text = "+" + rewardCoins.ToString();

    }

    private void OnContinueClicked()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        GameManager.Instance.AddCoins(rewardCoins);
        GameManager.Instance.LoadNextLevel();
        Hide();
    }
}
