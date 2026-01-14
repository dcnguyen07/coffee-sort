using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GoogleMobileAds.Api;
using static AdsControl;
using UnityEngine.Advertisements;

public class UIBuyItem : UIDialog
{
    [Header("UI Components")]
    public Text uiTextTitle;
    public Text uiTextDesc;
    public Text uiTextReward1; 
    public Text uiTextReward2;
    public Text uiTextCoin;
    public Image rewardIcon1;
    public Image rewardIcon2;
    public Sprite[] rewardSpriteArr;

    public Button uiBtnBuy;    
    public Button uiBtnWatch; 
    public Button uiBtnClose; 

    private int boosterAmountCoin = 1;
    private int boosterAmountVideo = 1; 
    private int boosterPrice = 2000;

    private BoosterButton.BoosterType currentBoosterType;

    private void Start()
    {
        uiBtnBuy.onClick.AddListener(BuyBoosterWithCoin);
        uiBtnWatch.onClick.AddListener(BuyBoosterWithVideo);
        uiBtnClose.onClick.AddListener(Hide);
        boosterAmountCoin = 1;
        boosterAmountVideo = 1;
        boosterPrice = 2000;
    }

    /// <summary>
    /// Show Dialog with booster data
    /// </summary>
    public void Show(BoosterButton.BoosterType boosterType)
    {
        base.Show();
        GameManager.Instance.currentState = GameState.InPause;
        currentBoosterType = boosterType;

        uiTextTitle.text = boosterType.ToString();
        uiTextDesc.text = "Add more " + boosterType.ToString().ToLower() + "s";
        uiTextReward1.text = boosterAmountCoin.ToString();
        uiTextReward2.text = "x " + boosterAmountVideo;
        uiTextCoin.text = boosterPrice.ToString();

        rewardIcon1.sprite = rewardSpriteArr[(int)boosterType];
        rewardIcon2.sprite = rewardSpriteArr[(int)boosterType];
    }

    /// <summary>
    /// Buy booster with coin
    /// </summary>
    private void BuyBoosterWithCoin()
    {
        if (GameManager.Instance.HasEnoughCoins(boosterPrice))
        {
            GameManager.Instance.SpendCoins(boosterPrice);
            GameManager.Instance.MoreBooster(boosterAmountCoin, currentBoosterType);
            Hide();
        }
        else
        {
            GameManager.Instance.uiManager.uiToast.ShowToast("Not enough coins!");
        }
    }

    /// <summary>
    /// Buy booster with video
    /// </summary>
    private void BuyBoosterWithVideo()
    {
        WatchAds();
    }

    public void WatchAds()
    {
        if (AdsControl.Instance.currentAdsType == ADS_TYPE.ADMOB)
        {
            if (AdsControl.Instance.rewardedAd != null)
            {
                if (AdsControl.Instance.rewardedAd.CanShowAd())
                {
                    AdsControl.Instance.ShowRewardAd(EarnReward);
                }
            }
        }
        else if (AdsControl.Instance.currentAdsType == ADS_TYPE.UNITY)
        {
            ShowRWUnityAds();
        }
        else if (AdsControl.Instance.currentAdsType == ADS_TYPE.MEDIATION)
        {
            if (AdsControl.Instance.rewardedAd.CanShowAd())

                AdsControl.Instance.ShowRewardAd(EarnReward);

            else
                ShowRWUnityAds();
        }
    }

    public void EarnReward(Reward reward)
    {
        GameManager.Instance.MoreBooster(boosterAmountVideo, currentBoosterType);
        RefreshBoosterUI();
        Hide();
    }

    public void ShowRWUnityAds()
    {
        AdsControl.Instance.PlayUnityVideoAd((string ID, UnityAdsShowCompletionState callBackState) =>
        {

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                GameManager.Instance.MoreBooster(boosterAmountVideo, currentBoosterType);
                Hide();
            }

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                AdsControl.Instance.LoadUnityAd();
            }

        });
    }

    private void RefreshBoosterUI()
    {
        if (currentBoosterType == BoosterButton.BoosterType.Slot)
        {
           GameManager.Instance.uiManager.uiGame.btnAddSlot.UpdateBoosterState();
        }
        else  if (currentBoosterType == BoosterButton.BoosterType.Order)
        {
             GameManager.Instance.uiManager.uiGame.btnOrder.UpdateBoosterState();
        }
        else if (currentBoosterType == BoosterButton.BoosterType.Undo)
        {
            GameManager.Instance.uiManager.uiGame.btnUndo.UpdateBoosterState();
        }
    }

    public void Hide()
    {
        base.Hide();
        GameManager.Instance.currentState = GameState.Playing;
    }
}
