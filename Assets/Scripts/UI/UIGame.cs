using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIGame : UIPanel
{
    [Header("Coin UI")]
    public Text txtCoin;
    public Button btnAddCoin;

    [Header("Level UI")]
    public Text txtLevelNormal;
    public Text txtLevelHard;
    public GameObject uiLevelNormal;
    public GameObject uiLevelHard;
    public GameObject uiHardLevelPanel;

    [Header("Helper Buttons")]
    [Header("Booster Buttons")]
    public BoosterButton btnAddSlot;
    public BoosterButton btnOrder;
    public BoosterButton btnUndo;
    public Button btnSetting; 

    [Header("Banner & Ads")]
    public GameObject bannerAd;

    [Header("Slot UI Elements")]
    public Text slotCountText;

    public void InitUI()
    {
        UpdateUI();
        InitializeBoosters();
    }

    /// <summary>
    /// Initialize boosters with initial state
    /// </summary>
    public void InitializeBoosters()
    {
        btnAddSlot.Initialize(GameManager.Instance.moreSlotsCount, 0);
        btnOrder.Initialize(GameManager.Instance.orderCount, 0); 
        btnUndo.Initialize(GameManager.Instance.undoCount, 0); 
    }

    private void Start()
    {
        btnSetting.onClick.AddListener(OnSetting);
        btnAddSlot.btnUse.onClick.AddListener(OnAddSlot);
        btnOrder.btnUse.onClick.AddListener(OnOrderly);
        btnUndo.btnUse.onClick.AddListener(OnUndo);
        btnAddCoin.onClick.AddListener(OnAddCoin);
    }

    /// <summary>
    /// Update UI when game starts
    /// </summary>
    public void UpdateUI()
    {
        txtLevelNormal.text = "Level " + GameManager.Instance.GetCurrentLevel();
        txtLevelHard.text = "Hard Level " + GameManager.Instance.GetCurrentHardLevel();

        bool isHardMode = GameManager.Instance.IsHardMode();
        uiLevelNormal.SetActive(!isHardMode);
        uiLevelHard.SetActive(isHardMode);
        uiHardLevelPanel.SetActive(isHardMode);
    }

    /// <summary>
    /// Handle event when adding coin button is pressed
    /// </summary>
    private void OnAddCoin()
    {
        // SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        // GameManager.Instance.uiManager.uiShop.Show();
        // GameManager.Instance.uiManager.uiShop.ShowEffect();
    }

    /// <summary>
    /// Handle event when adding slot button is pressed
    /// </summary>
    private void OnAddSlot()
    {

        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        if (GameManager.Instance.moreSlotsCount > 0)
        {
            Debug.Log("Thêm slot mới");
            GameManager.Instance.AddSlot();
            btnAddSlot.UseBooster();
            UpdateUI();
        }
        else
        {
            Debug.Log("More Slots");
            // GameManager.Instance.uiManager.uiBuyItem.Show(BoosterButton.BoosterType.Slot);           
        }
    }

    /// <summary>
    /// Handle event when pressing the order button
    /// </summary>
    private void OnOrderly()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        if (GameManager.Instance.orderCount > 0)
        {
            Debug.Log("Sắp xếp lại khay ly");
            GameManager.Instance.SortTrays();
            btnOrder.UseBooster();
            UpdateUI();
        }
        else
        {
            // GameManager.Instance.uiManager.uiBuyItem.Show(BoosterButton.BoosterType.Order);
            Debug.Log("More Slots");
        }
    }

    /// <summary>
    /// Handle event when undo button is pressed
    /// </summary>
    private void OnUndo()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        if (GameManager.Instance.undoCount > 0 && GameManager.Instance.CanUndo())
        {
            //Debug.Log("Hoàn tác nước đi");
            GameManager.Instance.OnUndo();
            btnUndo.UseBooster();
            UpdateUI();
        }
        else if(GameManager.Instance.undoCount == 0)
        {
            // GameManager.Instance.uiManager.uiBuyItem.Show(BoosterButton.BoosterType.Undo);
            Debug.Log("More Slots");
        }
        else{
             // GameManager.Instance.uiManager.uiToast.ShowToast("Can not undo", 1.5f);
        }
    }

    /// <summary>
    /// Handle event when setting button is pressed
    /// </summary>
    private void OnSetting()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        // GameManager.Instance.uiManager.uiSetting.InitUI();
        // GameManager.Instance.uiManager.uiSetting.Show();
    }

    /// <summary>
    /// Show UI Game when entering the game.
    /// </summary>
    public override void Show()
    {
        base.Show();
    }

    /// <summary>
    /// Hide UI Game when switching to another screen.
    /// </summary>
    public override void Hide()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        base.Hide();
        //Debug.Log("UIGame ẩn đi!");
    }

    /// <summary>
    /// Update the number of slots displayed on the UI
    /// </summary>
    /// <param name="remainingSlots">Number of remaining slots</param>
    public void UpdateSlotDisplay(int remainingSlots)
    {
        if (slotCountText != null)
        {
            slotCountText.text = remainingSlots.ToString();
        }

        if (remainingSlots == 0)
        {
            btnAddSlot.gameObject.SetActive(false);
        }
    }

}