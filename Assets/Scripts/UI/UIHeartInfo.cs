using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using static AdsControl;

public class UIHeartInfo : UIDialog
{
    [SerializeField] private Text uiHeartCount;
    [SerializeField] private Text uiTxtFull;
    [SerializeField] private GameObject uiTimerObject;
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject buyBtnNode;
    [SerializeField] private Button uiBtnAd;
    [SerializeField] private Button uiBtnCoin;
    [SerializeField] private Button uiBtnClose;

    private void Start()
    {
        uiBtnAd.onClick.AddListener(OnWatchAd);
        uiBtnCoin.onClick.AddListener(OnBuyHeart);
        uiBtnClose.onClick.AddListener(Hide);
    }

    public override void Show()
    {
        base.Show();
        UpdateHeartUI();
    }

    /// <summary>
    /// Update UI Heart Info
    /// </summary>
    private void UpdateHeartUI()
    {
        int currentHeart = HeartManager.Instance.GetHeart();
        uiHeartCount.text = currentHeart.ToString();

        if (currentHeart >= 5)
        {
            buyBtnNode.SetActive(false);
            uiTxtFull.gameObject.SetActive(true);
            uiTimerObject.SetActive(false);
        }
        else
        {
            buyBtnNode.SetActive(true);
            uiTxtFull.gameObject.SetActive(false);
            uiTimerObject.SetActive(HeartManager.Instance.HasHeartTimer());

            if (uiTimerObject.activeSelf)
            {
                UpdateTimer();
            }
        }
    }

    private void UpdateTimer()
    {
        int remainingTime = HeartManager.Instance.GetHeartTimer();
        timerText.text = FormatTime(remainingTime);
        InvokeRepeating(nameof(UpdateCountdown), 1f, 1f);
    }

    private void UpdateCountdown()
    {
        int remainingTime = HeartManager.Instance.GetHeartTimer();
        timerText.text = FormatTime(remainingTime);

        if (remainingTime <= 0)
        {
            CancelInvoke(nameof(UpdateCountdown));
            HeartManager.Instance.AddHeart();
            UpdateHeartUI();
        }
    }

    private string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int sec = seconds % 60;
        return $"{minutes:D2}:{sec:D2}";
    }

    private void OnWatchAd()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        WatchAds();
    }

    private void OnBuyHeart()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        if (GameManager.Instance.GetCoin() >= 500)
        {
            GameManager.Instance.SpendCoins(500);
            HeartManager.Instance.RefillHeart();
            UpdateHeartUI();
        }
    }

    private void OnClaimFreeHeart()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        WatchAds();
    }

    public override void Hide()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickSfx);
        base.Hide();
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
        HeartManager.Instance.AddHeart();
        UpdateHeartUI();
    }

    public void ShowRWUnityAds()
    {
        AdsControl.Instance.PlayUnityVideoAd((string ID, UnityAdsShowCompletionState callBackState) =>
        {

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                HeartManager.Instance.AddHeart();
                UpdateHeartUI();
            }

            if (ID.Equals(AdsControl.Instance.adUnityRWUnitId) && callBackState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                AdsControl.Instance.LoadUnityAd();
            }

        });
    }
}