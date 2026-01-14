using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public UIGame uiGame;
    public UIHome uiHome;
    public UIShop uiShop;
    public UIShop uiShopTab;

    [Header("UI Dialogs")]
    public UIDialog uiPauseDialog;
    public UIDialog uiConfirmDialog;
    public UIDialog uiRewardDialog;
    public UIDialog uiMoreHeartDialog;
    public UISetting uiSetting;
    public UIRevive uiRevive;
    public UIGameFail uiGameFail;
    public UIGameSuccess uiGameSuccess;
    public BarHeartManager barHeartManager;
    public UIHeartInfo uiHeartInfo;
    public UILobbyRankList uiLobby;
    public UIToast uiToast;
    public UIBuyItem uiBuyItem;

    public void ShowPanel(UIPanel panel)
    {
        HideAllPanels();
        panel.Show();
    }

    public void HideAllPanels()
    {
        if (uiGame != null)
            uiGame.Hide();

        if (uiHome != null)
            uiHome.Hide();

        if (uiSetting != null)
            uiSetting.Hide();

        if (uiShop != null)
            uiShop.Hide();
    }

    public void ShowDialog(UIDialog dialog)
    {
        dialog.Show();
    }

    public void HideDialog(UIDialog dialog)
    {
        dialog.Hide();
    }
}