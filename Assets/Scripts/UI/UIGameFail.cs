using UnityEngine.UI;
using Managers;

public class UIGameFail : UIDialog
{
    public Button btnReplay;
    public Button btnClose;

    private void Start()
    {
        btnReplay.onClick.AddListener(OnReplay);
    }

    private void OnReplay()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        GameManager.instance.RestartLevel();
        HideWithEffect();
    }
}