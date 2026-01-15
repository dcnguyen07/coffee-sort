using Managers;
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
   
    /// <summary>
    /// Initialize booster with count and required level
    /// </summary>
    public void Initialize(int initialCount, int requiredLevel)
    {
        unlockLevel = requiredLevel;
        boosterCount = initialCount;
    }


    /// <summary>
    /// Use booster if there are remaining uses
    /// </summary>
    public void UseBooster()
    {
        boosterCount = GameManager.instance.GetBoosterCount(currentType);
        GameManager.instance.SpendBooster(1, currentType);
        StartCoroutine(GameManager.instance.boardManager.ProcessCupQueue());
        Debug.Log("Booster Used!");
    }
    
    /// <summary>
    /// Add more booster
    /// </summary>
    public void AddMoreBooster()
    {
        GameManager.instance.MoreBooster(1, currentType);
        boosterCount = GameManager.instance.GetBoosterCount(currentType);

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