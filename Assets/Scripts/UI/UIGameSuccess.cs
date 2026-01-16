using UnityEngine;
using UnityEngine.UI;
using Managers;
using TMPro;

public class UIGameSuccess : UIDialog
{
    [SerializeField] private TextMeshProUGUI uiLevel;
    [SerializeField] private Button btnContinue;

    public void InitUI()
    {
        uiLevel.text = "Level " + GameManager.instance.GetCurrentLevel();
    }

    private void Start()
    {
        btnContinue.onClick.AddListener(OnContinueClicked);
    }

    public void ShowSuccess(int level)
    {
        uiLevel.text = "Level " + level;
    }

    private void OnContinueClicked()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        GameManager.instance.LoadNextLevel();
        Hide();
    }
}
