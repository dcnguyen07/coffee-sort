using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIHome : UIPanel
{
    public RectTransform tabBg;
    

    [Header("Game UI Elements")]
    public Text levelText;
    public Button playButton;
    public Button btnSetting;
    public Button btnHeart;
    public Button btnCoin;

    public void InitUI()
    {
        UpdateLevelText(GameManager.Instance.GetCurrentLevel());
    }

    private void Start()
    {
       
    }

    private void OnSetting()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
    }

    private void OnMoreHeart()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
    }

    private void OnPlayButtonClicked()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        Hide(); 
        GameManager.Instance.uiManager.uiGame.Show(); 
        GameManager.Instance.LoadGame();
    }

    public void UpdateLevelText(int level)
    {
        levelText.text = level.ToString();
    }
}
