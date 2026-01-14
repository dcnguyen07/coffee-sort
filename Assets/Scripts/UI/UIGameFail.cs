using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class UIGameFail : UIDialog
{
    public Button btnReplay;
    public Button btnClose;

    private void Start()
    {
        btnReplay.onClick.AddListener(OnReplay);
        btnClose.onClick.AddListener(OnClose);
    }

    private void OnReplay()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        HeartManager.Instance.SpendHeart();
        GameManager.Instance.RestartLevel();
        HideWithEffect();
    }

    private void OnClose()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        HeartManager.Instance.SpendHeart();
        HideWithEffect();
        GameManager.Instance.boardManager.ClearBoard();
        GameManager.Instance.uiManager.uiGame.Hide();
        Hide();
        GameManager.Instance.uiManager.uiHome.InitUI();
        GameManager.Instance.uiManager.uiHome.Show();
    }

    public override void Show()
    {
        base.Show();
    }

    IEnumerator ShowAdsIE()
    {
        yield return new WaitForSeconds(1.0f);

        if (GameManager.Instance.GetCurrentLevel() > 1)
        {
            AdsControl.Instance.ShowInterstitialAd();
        }

    }

}