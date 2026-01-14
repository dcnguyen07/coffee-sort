using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoosterButton : MonoBehaviour
{
    public enum BoosterType { Slot, Order, Undo }

    public enum BoosterState { Locked, Unlocked, AddMore }

    [Header("UI Elements")]
    public Button btnUse;
    public Button btnUnlock;
    public GameObject itemCount;
    public Text txtCount;
    public GameObject addImage;

    public GameObject disableMask;

    private BoosterState currentState;
    public BoosterType currentType;
    private int boosterCount = 0;
    private int unlockLevel;

    private void Awake()
    {

    }

    /// <summary>
    /// Initialize booster with count and required level
    /// </summary>
    public void Initialize(int initialCount, int requiredLevel)
    {
        unlockLevel = requiredLevel;
        boosterCount = initialCount;
        UpdateBoosterState();
    }

    /// <summary>
    /// Update booster state based on count and player level
    /// </summary>
    public void UpdateBoosterState()
    {
        int currentLevel = GameManager.Instance.GetCurrentLevel();
        boosterCount = GameManager.Instance.GetBoosterCount(currentType);
        if (currentLevel < unlockLevel)
        {
            // Debug.Log("Lock");
            SetState(BoosterState.Locked);
        }
        else if (boosterCount > 0)
        {
            // Debug.Log("Unlock");
            SetState(BoosterState.Unlocked);
        }
        else if (boosterCount == 0)
        {
            // Debug.Log("Add More");
            SetState(BoosterState.AddMore);
        }
    }

    /// <summary>
    /// Set new state for booster
    /// </summary>
    private void SetState(BoosterState newState)
    {
        currentState = newState;

        btnUse.gameObject.SetActive(newState == BoosterState.Unlocked || newState == BoosterState.AddMore);
        //btnUse.interactable = newState == BoosterState.Unlocked;

        btnUnlock.gameObject.SetActive(newState == BoosterState.Locked);
        itemCount.SetActive(newState == BoosterState.Unlocked);
        txtCount.text = boosterCount.ToString();
        addImage.SetActive(newState == BoosterState.AddMore);
    }

    /// <summary>
    /// Use booster if there are remaining uses
    /// </summary>
    public void UseBooster()
    {
        if (currentState != BoosterState.Unlocked) return;
        boosterCount = GameManager.Instance.GetBoosterCount(currentType);
        boosterCount--;
        GameManager.Instance.SpendBooster(1, currentType);
        StartCoroutine(GameManager.Instance.boardManager.ProcessCupQueue());
        UpdateBoosterState();
        Debug.Log("Booster Used!");
    }

    /// <summary>
    /// Unlock booster when conditions are met
    /// </summary>
    private void UnlockBooster()
    {
        if (GameManager.Instance.GetCurrentLevel() >= unlockLevel)
        {
            UpdateBoosterState();
        }
    }

    /// <summary>
    /// Add more booster
    /// </summary>
    public void AddMoreBooster()
    {
        GameManager.Instance.MoreBooster(1, currentType);
        boosterCount = GameManager.Instance.GetBoosterCount(currentType);
        UpdateBoosterState();

    }

    public void DisableBtn()
    {
        if (currentState == BoosterState.Unlocked)
        {
            disableMask.SetActive(true);
            btnUse.interactable = false;
            btnUnlock.interactable = false;
        }

    }
    public void EnableBtn()
    {
        if (currentState == BoosterState.Unlocked)
        {
            disableMask.SetActive(false);
            btnUse.interactable = true;
            btnUnlock.interactable = true;
        }
    }
}