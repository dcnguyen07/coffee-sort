using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIHome : UIPanel
{
    [Header("Tab System")]
    public TabItem tabStore;
    public TabItem tabHome;
    public TabItem tabRank;
    public RectTransform tabBg;
    public RectTransform contentPanel;

    private TabItem[] tabItems;
    private int currentTab = 1;
    private RectTransform[] panels;

    [Header("Game UI Elements")]
    public Text levelText;
    public Button playButton;
    public Button btnSetting;
    public Button btnHeart;
    public Button btnCoin;

    public void InitUI()
    {
        UpdateLevelText(GameManager.Instance.GetCurrentLevel());
        GameManager.Instance.uiManager.uiLobby.InitUI();
    }

    private void Start()
    {
        tabItems = new TabItem[] { tabStore, tabHome, tabRank };
        panels = new RectTransform[] { tabStore.panel, tabHome.panel, tabRank.panel };
        currentTab = 1;
        foreach (var tab in tabItems)
        {
            tab.btnHandler.onClick.AddListener(() => OnTabClicked(tab));
        }

        playButton.onClick.AddListener(OnPlayButtonClicked);
        btnSetting.onClick.AddListener(() => OnSetting());
        btnHeart.onClick.AddListener(() => OnMoreHeart());
        btnCoin.onClick.AddListener(() => OnTabClicked(tabStore)); 
    }

    private void OnSetting()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        GameManager.Instance.uiManager.uiSetting.InitUI();
        GameManager.Instance.uiManager.uiSetting.Show();
    }

    private void OnMoreHeart()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        GameManager.Instance.uiManager.uiHeartInfo.Show();
    }

    private void OnTabClicked(TabItem tab)
    {
        if (tab == tabItems[currentTab]) return;

        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        int tabIndex = System.Array.IndexOf(tabItems, tab);
        UpdateTabUI(tabIndex);

        float targetX = -panels[tabIndex].anchoredPosition.x;
        contentPanel.DOAnchorPosX(targetX, 0.3f).SetEase(Ease.OutQuad);
        if (tabIndex == 0)
        {
            GameManager.Instance.uiManager.uiShopTab.ShowEffect();
        }
        currentTab = tabIndex;
    }

    private void UpdateTabUI(int tabIndex)
    {
        TabItem selectedTab = tabItems[tabIndex];

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tabBg.parent as RectTransform,
            selectedTab.icon.position,
            null,
            out localPoint
        );
        tabBg.DOAnchorPosX(localPoint.x, 0.3f).SetEase(Ease.OutQuad);
        Debug.Log(selectedTab.icon.anchoredPosition.x);
        foreach (var tab in tabItems)
        {
            if (tab == selectedTab)
            {
                tab.icon.DOAnchorPosY(150, 0.3f);
                tab.text.gameObject.SetActive(true);
            }
            else
            {
                tab.icon.DOAnchorPosY(64, 0.3f); 
                tab.text.gameObject.SetActive(false);
            }
        }
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

[System.Serializable]
public class TabItem
{
    public string tabName;
    public RectTransform panel;
    public Button btnHandler;
    public RectTransform icon;
    public Text text;
}