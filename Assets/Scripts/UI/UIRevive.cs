using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using GoogleMobileAds.Api;
using static AdsControl;
using UnityEngine.Advertisements;

public class UIRevive : UIDialog
{
    public Button btnPlayByAd;
    public Button btnPlayByCoin;
    public Button btnClose;

    private void Start()
    {
        btnPlayByAd.onClick.AddListener(OnPlayByAd);
        btnPlayByCoin.onClick.AddListener(OnPlayByCoin);
        btnClose.onClick.AddListener(OnClose);
    }

    private void OnPlayByAd()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        WatchAds();

    }

    private void OnPlayByCoin()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        if (GameManager.Instance.GetCoin() >= 900)
        {
            GameManager.Instance.SpendCoins(900);
            GameManager.Instance.UnlockSlot();
            HideWithEffect();
        }
        else
        {
            GameManager.Instance.uiManager.uiShop.Show();
            GameManager.Instance.uiManager.uiShop.ShowEffect();
        }
    }

    private void OnClose()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        HideWithEffect();
        GameManager.Instance.uiManager.uiGameFail.Show();

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
        GameManager.Instance.UnlockSlot();
        HideWithEffect();
    }

    public void ShowRWUnityAds()
    {
        AdsControl.Instance.PlayUnityVideoAd((string ID, UnityAdsShowCompletionState callBackState) =>
        {

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                GameManager.Instance.UnlockSlot();
                HideWithEffect();
            }

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                AdsControl.Instance.LoadUnityAd();
            }

        });
    }
}