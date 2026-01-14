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
        GameManager.Instance.RestartLevel();
        HideWithEffect();
    }

    private void OnClose()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        HideWithEffect();
        GameManager.Instance.boardManager.ClearBoard();
        GameManager.Instance.uiManager.uiGame.Hide();
        Hide();
    }

    public override void Show()
    {
        base.Show();
    }

}