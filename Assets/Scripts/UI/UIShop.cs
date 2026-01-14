using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using DG.Tweening;
using GoogleMobileAds.Api;
using static AdsControl;
using UnityEngine.Advertisements;

public class UIShop : UIPanel
{
    [Header("Shop Items")]
    public List<ShopItem> shopItems; 
    public List<Transform> shopItemObjects;

    public Button shopPack;

    public Button closeBtn;

    public Button restoreBtn;

    [Header("Animate Items")]
    [SerializeField] private GameObject[] shopAnimateItems; 
    [SerializeField] private float itemDelay = 0.05f; 
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease animationEase = Ease.OutBack;

    private void Start()
    {
        LoadShopItems();
    }

    private void LoadShopItems()
    {
        shopPack.onClick.AddListener(() => BuyPack());
        restoreBtn.onClick.AddListener(() => Restore());
        closeBtn.onClick.AddListener(() => Hide());

        for (int i = 0; i < shopItems.Count; i++)
        {
            if (i >= shopItemObjects.Count)
                continue;

            ShopItem item = shopItems[i];
            Transform itemUI = shopItemObjects[i];

            //itemUI.Find("BG/Icon").GetComponent<Image>().sprite = item.icon;
            itemUI.Find("BG/uiTxtValue").GetComponent<Text>().text = item.coinAmount > 0 ? $"x{item.coinAmount}" : "";
            itemUI.Find("BG/uiBtnPurchase/uiTxtValue").GetComponent<Text>().text = item.isPurchased ? "Owned" : $"${item.price}";

            Button purchaseButton = itemUI.Find("BG/uiBtnPurchase").GetComponent<Button>();
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(() => PurchaseItem(item, itemUI));

            // Xử lý Countdown Timer
            if (item.isLimitedTime)
            {
                itemUI.Find("BG/uiBtnPurchase/uiTxtValue").GetComponent<Text>().text = "Free";
                StartCoroutine(HandleLimitedTimeItem(item, itemUI));
            }
        }
    }

    public void ShowEffect()
    {
        
        foreach (var item in shopAnimateItems)
        {
            item.transform.localScale = Vector3.zero;
            item.gameObject.SetActive(false);
        }

        ShowShop();
    }

    /// <summary>
    /// Show UI Shop with effect
    /// </summary>
    public void ShowShop()
    {
        StartCoroutine(AnimateShopItems());
    }

    private IEnumerator AnimateShopItems()
    {
        for (int i = 0; i < shopAnimateItems.Length; i++)
        {
            shopAnimateItems[i].gameObject.SetActive(true);
            shopAnimateItems[i].transform.DOScale(Vector3.one, animationDuration)
                .SetEase(animationEase);
            yield return new WaitForSeconds(itemDelay);
        }
    }

    private void BuyPack()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
    }

    private void Restore()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
    }

    private IEnumerator HandleLimitedTimeItem(ShopItem item, Transform itemUI)
    {
        Transform cooldownUI = itemUI.Find("BG/Cooldown");
        Text cooldownText = cooldownUI.Find("Text").GetComponent<Text>();
        Button purchaseButton = itemUI.Find("BG/uiBtnPurchase").GetComponent<Button>();

        while (true)
        {
            float remainingTime = item.GetRemainingTime();

            if (remainingTime > 0)
            {
                purchaseButton.gameObject.SetActive(false);
                cooldownUI.gameObject.SetActive(true);
                cooldownText.text = FormatTime(remainingTime);
            }
            else
            {
                purchaseButton.gameObject.SetActive(true);
                cooldownUI.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void PurchaseItem(ShopItem item, Transform itemUI)
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        if (item.isPurchased)
        {
            Debug.Log($"{item.itemName} đã được mua trước đó!");
            return;
        }

        if (item.isLimitedTime)
        { 
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            PlayerPrefs.SetString(item.GetLastPurchaseKey(), currentTime.ToString());
            PlayerPrefs.Save();
            WatchAds();
            StartCoroutine(HandleLimitedTimeItem(item, itemUI)); // Bắt đầu countdown
        }
        else
        {
            if (item.itemId == Config.IAPPackageID.NoAdsPack)
            {
                RemoveAds();
            }
            else if (item.coinAmount > 0)
            {
                BuyIAPPackage(item.itemId);
            }
        }

        Debug.Log($"Mua thành công: {item.itemName}");
    }

    private void RemoveAds()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        BuyIAPPackage(Config.IAPPackageID.NoAdsPack);
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes:D2}:{secs:D2}";
    }

    public override void Show()
    {
        base.Show();
        GameManager.Instance.currentState = GameState.InPause;
    }

    public override void Hide()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        base.Hide();
        GameManager.Instance.currentState = GameState.Playing;
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
        GameManager.Instance.AddCoins(50);
    }

    public void ShowRWUnityAds()
    {
        AdsControl.Instance.PlayUnityVideoAd((string ID, UnityAdsShowCompletionState callBackState) =>
        {

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                GameManager.Instance.AddCoins(50);
            }

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                AdsControl.Instance.LoadUnityAd();
            }

        });
    }

    public void BuyIAPPackage(Config.IAPPackageID packageID)
    {       
        IAPManager.instance.BuyConsumable(packageID, (string iapID, IAPManager.IAP_CALLBACK_STATE state) =>
        {
            if (state == IAPManager.IAP_CALLBACK_STATE.SUCCESS)
            {

                Debug.Log("SUCCESSSUCCESS " + iapID);

                if (iapID.Equals(Config.IAPPackageID.NoAdsPack.ToString()))
                {
                    //Debug.Log("REMOVE ADS");
                    GameManager.Instance.AddCoins(2000);
                    AdsControl.Instance.RemoveAds();
                }
                else if (iapID.Equals(Config.IAPPackageID.NoAds.ToString()))
                {
                    //Debug.Log("REMOVE ADS");
                    AdsControl.Instance.RemoveAds();
                }
                else
                {
                    BuySuccesss(packageID);
                }
            }
            else
            {
                Debug.Log("Buy Fail!");

            }
        });



    }

    public void BuySuccesss(Config.IAPPackageID packageID)
    {
        switch (packageID)
        {

            case Config.IAPPackageID.coin_1000:
                GameManager.Instance.AddCoins(1000);
                break;

            case Config.IAPPackageID.coin_5000:
                GameManager.Instance.AddCoins(5000);
                break;

            case Config.IAPPackageID.coin_10000:
                GameManager.Instance.AddCoins(10000);
                break;

            case Config.IAPPackageID.coin_25000:
                GameManager.Instance.AddCoins(25000);
                break;
        }
    }
}

[System.Serializable]
public class ShopItem
{
    public Config.IAPPackageID itemId;
    public string itemName; 
    public Sprite icon; 
    public int coinAmount; 
    public float price;
    public bool isLimitedTime;
    public bool isPurchased;
    private const int cooldownDuration = 3600;

    public string GetLastPurchaseKey() => $"LastPurchase_{itemId}";

    public float GetRemainingTime()
    {
        if (!PlayerPrefs.HasKey(GetLastPurchaseKey()))
            return 0;

        long lastPurchaseTime = long.Parse(PlayerPrefs.GetString(GetLastPurchaseKey(), "0"));
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        long elapsedTime = currentTime - lastPurchaseTime;
        return Mathf.Max(cooldownDuration - elapsedTime, 0);
    }
}