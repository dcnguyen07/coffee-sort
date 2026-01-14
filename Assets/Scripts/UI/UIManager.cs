using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")] public UIGame uiGame;
    
    public UIGameFail uiGameFail;
    public UIGameSuccess uiGameSuccess;

    public void ShowPanel(UIPanel panel)
    {
        HideAllPanels();
        panel.Show();
    }

    public void HideAllPanels()
    {
        if (uiGame != null)
            uiGame.Hide();
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