using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UIGame : UIPanel
{
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
        btnAddSlot.Initialize(GameManager.instance.moreSlotsCount, 0);
        btnOrder.Initialize(GameManager.instance.orderCount, 0); 
        btnUndo.Initialize(GameManager.instance.undoCount, 0); 
    }

    private void Start()
    {
        btnAddSlot.btnUse.onClick.AddListener(OnAddSlot);
        btnOrder.btnUse.onClick.AddListener(OnOrderly);
        btnUndo.btnUse.onClick.AddListener(OnUndo);
    }

    /// <summary>
    /// Update UI when game starts
    /// </summary>
    public void UpdateUI()
    {
        txtLevelNormal.text = "Level " + GameManager.instance.GetCurrentLevel();
        bool isHardMode = GameManager.instance.IsHardMode();
        uiLevelNormal.SetActive(!isHardMode);
    }

    /// <summary>
    /// Handle event when adding slot button is pressed
    /// </summary>
    private void OnAddSlot()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
            GameManager.instance.AddSlot();
            btnAddSlot.UseBooster();
    }

    /// <summary>
    /// Handle event when pressing the order button
    /// </summary>
    private void OnOrderly()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
            Debug.Log("Sắp xếp lại khay ly");
            GameManager.instance.SortTrays();
            btnOrder.UseBooster();
    }

    /// <summary>
    /// Handle event when undo button is pressed
    /// </summary>
    private void OnUndo()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
            GameManager.instance.OnUndo();
            btnUndo.UseBooster();
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