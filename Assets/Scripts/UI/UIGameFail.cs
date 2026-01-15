using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Managers;

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
        GameManager.instance.RestartLevel();
        HideWithEffect();
    }

    private void OnClose()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        HideWithEffect();
        GameManager.instance.boardManager.ClearBoard();
        GameManager.instance.uiManager.uiGame.Hide();
        Hide();
    }

    public override void Show()
    {
        base.Show();
    }

}