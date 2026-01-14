using UnityEngine;
using UnityEngine.UI;

public class UISetting : UIDialog
{
    [Header("Buttons")]
    public Button btnClose;
    public Button btnHome;
    public Button btnSound;
    public Button btnMusic;
    public Button btnShock;
    public Button btnRemoveAds;
    public Button btnRestorePurchase;
    public Button btnTerm;
    public Button btnPolicy;

    [Header("Icons")]
    public Image iconSound;
    public Image iconMusic;
    public Image iconShock;

    [Header("Sprites")]
    public Sprite spriteOn;
    public Sprite spriteOff;

    private void Start()
    {
        // Gán sự kiện nút
        btnClose.onClick.AddListener(Hide);
        btnHome.onClick.AddListener(OnHomePressed);
        btnSound.onClick.AddListener(ToggleSound);
        btnMusic.onClick.AddListener(ToggleMusic);
        btnShock.onClick.AddListener(ToggleShock);
        btnRemoveAds.onClick.AddListener(RemoveAds);
        btnRestorePurchase.onClick.AddListener(RestorePurchase);
        btnTerm.onClick.AddListener(Term);
        btnPolicy.onClick.AddListener(Policy);

        UpdateUI();
    }

    public void InitUI()
    {
        if (GameManager.Instance.currentState == GameState.Playing)
            Show(true);
        else
            Show(false);
    }

    /// <summary>
    /// Update UI when opening Setting
    /// </summary>
    public void Show(bool isGameScene)
    {
        base.Show();
        if (GameManager.Instance.currentState == GameState.Playing)
            GameManager.Instance.currentState = GameState.InPause;
        btnHome.gameObject.SetActive(isGameScene);
    }

    /// <summary>
    /// Update UI of sound, music, shock
    /// </summary>
    private void UpdateUI()
    {
        iconSound.sprite = GameManager.Instance.IsSoundOn() ? spriteOn : spriteOff;
        iconMusic.sprite = GameManager.Instance.IsMusicOn() ? spriteOn : spriteOff;
        iconShock.sprite = GameManager.Instance.IsShockOn() ? spriteOn : spriteOff;
    }

    /// <summary>
    /// Handle toggle sound
    /// </summary>
    private void ToggleSound()
    {
        GameManager.Instance.ToggleSound();
        UpdateUI();
    }

    /// <summary>
    /// Handle toggle music
    /// </summary>
    private void ToggleMusic()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        GameManager.Instance.ToggleMusic();
        UpdateUI();
    }

    /// <summary>
    /// Handle toggle shock
    /// </summary>
    private void ToggleShock()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        GameManager.Instance.ToggleShock();
        UpdateUI();
    }

    /// <summary>
    /// Handle Remove Ads
    /// </summary>
    private void RemoveAds()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
    }

    /// <summary>
    /// Handle Restore Purchase
    /// </summary>
    private void RestorePurchase()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
    }

    /// <summary>
    /// Handle Home Button → Return to UI Home
    /// </summary>
    private void OnHomePressed()
    {
        //SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        Hide();
        GameManager.Instance.ReturnToHome();
    }

    public override void Hide()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        base.Hide();
    }

    /// <summary>
    /// Opens the Terms of Service URL
    /// </summary>
    public void Term()
    {
        string termsUrl = "https://www.google.com"; // Replace with actual URL
        Application.OpenURL(termsUrl);
    }

    /// <summary>
    /// Opens the Privacy Policy URL
    /// </summary>
    public void Policy()
    {
        string policyUrl = "https://www.google.com"; // Replace with actual URL
        Application.OpenURL(policyUrl);
    }
}