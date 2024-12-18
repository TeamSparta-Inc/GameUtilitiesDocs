using System;
using System.Collections.Generic;
using UnityEngine;
using JungWoo.Utilities;
// using Firebase.Analytics;

public class AdsManager : MonoSingleton<AdsManager>
{
    [Header("Ad Unit IDs")]
    [Tooltip("보상형 광고의 유닛 ID")]
    [SerializeField] private string rewardedAdUnitId = "YOUR_REWARDED_AD_UNIT_ID";

    [Tooltip("전면 광고의 유닛 ID")]
    [SerializeField] private string interstitialAdUnitId = "YOUR_INTERSTITIAL_AD_UNIT_ID";

    [Header("Ad Limits")]
    [Tooltip("광고별 일일 노출 제한 설정")]
    [SerializeField] private List<AdLimitConfig> adLimitConfigs = new List<AdLimitConfig>();

    // 보상형 광고 보상 콜백
    private Action<MaxSdk.Reward, MaxSdkBase.AdInfo> pendingRewardAction;

    // 오늘 노출된 광고 횟수를 저장하는 딕셔너리
    private Dictionary<string, int> adsShownToday;

    // 마지막으로 광고를 보여준 날짜
    private DateTime lastAdShownDate;

    // 재시도 횟수
    private int rewardedRetryCount = 0;
    private const int maxRetryAttempts = 3;

    private bool isInitialized = false;
    private bool isRewardedAdLoading = false;
    private bool isInterstitialAdLoading = false;

    [Serializable]
    public class AdLimitConfig
    {
        public string adKey;     // 광고 키
        public int dailyLimit;   // 일일 노출 제한
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            InitializeRewardedAds();
            InitializeInterstitialAds();
            isInitialized = true;
        };

        MaxSdk.InitializeSdk();

        LoadAdData();
        UpdateAdTracking();
    }

    #region Data Management

    private void LoadAdData()
    {
        adsShownToday = ES3.Load(Consts.ADS_SHOWN_TODAY_SAVE, new Dictionary<string, int>());
        lastAdShownDate = ES3.Load(Consts.LAST_AD_SHOW_DATE_SAVE, DateTime.Today);

        // 기본값으로 초기화
        if (adsShownToday == null)
        {
            adsShownToday = new Dictionary<string, int>();
        }
    }

    #endregion

    #region Ad Limit Check

    public int GetAdShownCount(string adKey)
    {
        if (!adsShownToday.ContainsKey(adKey))
            adsShownToday[adKey] = 0;

        return adsShownToday[adKey];
    }

    // 광고 노출 제한 체크
    private bool CanShowAd(string adKey)
    {
        if (!adsShownToday.ContainsKey(adKey))
            adsShownToday[adKey] = 0;

        var config = adLimitConfigs.Find(c => c.adKey == adKey);
        if (config != null && adsShownToday[adKey] >= config.dailyLimit)
        {
            Debug.LogWarning($"Ad limit reached for {adKey}. Daily limit: {config.dailyLimit}");
            return false;
        }

        return true;
    }

    // 광고 노출 카운트 업데이트
    private void UpdateAdShownCount(string adKey)
    {
        if (!adsShownToday.ContainsKey(adKey))
            adsShownToday[adKey] = 0;

        adsShownToday[adKey]++;
        ES3.Save(Consts.ADS_SHOWN_TODAY_SAVE, adsShownToday);
        ES3.Save(Consts.LAST_AD_SHOW_DATE_SAVE, DateTime.Today);
    }

    private void UpdateAdTracking()
    {
        if (lastAdShownDate.Date != DateTime.Today)
        {
            adsShownToday.Clear();
            lastAdShownDate = DateTime.Today;
            ES3.Save(Consts.ADS_SHOWN_TODAY_SAVE, adsShownToday);
            ES3.Save(Consts.LAST_AD_SHOW_DATE_SAVE, lastAdShownDate);
        }
    }

    #endregion

    #region Rewarded Ads

    private void InitializeRewardedAds()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailed;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHidden;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedReward;

        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        if (isRewardedAdLoading || MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
            return;

        isRewardedAdLoading = true;
        MaxSdk.LoadRewardedAd(rewardedAdUnitId);
    }

    public bool ShowRewardedAd(string adKey, Action<MaxSdk.Reward, MaxSdkBase.AdInfo> rewardCallback)
    {
        if (!CanShowAd(adKey))
        {
            Debug.LogWarning("Cannot show rewarded ad. Daily limit reached.");
            return false;
        }

        if (MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
        {
            pendingRewardAction = rewardCallback;
            MaxSdk.ShowRewardedAd(rewardedAdUnitId);
            UpdateAdShownCount(adKey);
            return true;
        }
        else
        {
            Debug.LogWarning("Rewarded Ad not ready. Loading now...");
            LoadRewardedAd();
            ShowLoadingIndicator();
            ShowAdLoadFailedMessage("보상형");
            return false;
        }
    }


    private void OnRewardedAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isRewardedAdLoading = false;
    }

    private void OnRewardedAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        isRewardedAdLoading = false;
        Debug.LogError($"보상형 광고 로드 실패: {errorInfo.Message}");

        if (rewardedRetryCount < maxRetryAttempts)
        {
            rewardedRetryCount++;
            float retryDelay = Mathf.Pow(2, rewardedRetryCount);
            Invoke(nameof(LoadRewardedAd), retryDelay);
        }
        else
        {
            Debug.LogError("보상형 광고 로드 최대 재시도 횟수에 도달했습니다.");
        }
    }

    private void OnRewardedAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadRewardedAd();  // 광고를 본 후 새로운 광고를 미리 로드
    }

    private void OnRewardedAdReceivedReward(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        pendingRewardAction?.Invoke(reward, adInfo);
        pendingRewardAction = null;
    }

    #endregion

    #region Interstitial Ads

    private void InitializeInterstitialAds()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialAdLoaded;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialAdLoadFailed;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialAdHidden;

        LoadInterstitialAd();
    }

    private void LoadInterstitialAd()
    {
        if (isInterstitialAdLoading || MaxSdk.IsInterstitialReady(interstitialAdUnitId))
            return;

        isInterstitialAdLoading = true;
        MaxSdk.LoadInterstitial(interstitialAdUnitId);
    }

    public bool ShowInterstitialAd(string adKey)
    {
        if (!CanShowAd(adKey))
        {
            Debug.LogWarning("Cannot show interstitial ad. Daily limit reached.");
            return false;
        }

        if (MaxSdk.IsInterstitialReady(interstitialAdUnitId))
        {
            MaxSdk.ShowInterstitial(interstitialAdUnitId);
            UpdateAdShownCount(adKey);
            return true;
        }
        else
        {
            Debug.LogWarning("Interstitial Ad not ready. Loading now...");
            LoadInterstitialAd();
            return false;
        }
    }

    private void OnInterstitialAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isInterstitialAdLoading = false;
    }

    private void OnInterstitialAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        isInterstitialAdLoading = false;
        Debug.LogError($"Interstitial ad failed to load: {errorInfo.Message}");
    }

    private void OnInterstitialAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        LoadInterstitialAd();  // 광고를 본 후 새로운 광고를 미리 로드
    }

    #endregion

    #region Helpers

    private void ShowLoadingIndicator()
    {
        Debug.Log("Loading Ad...");
        // 여기에 로딩 UI 표시 코드를 추가하세요.
    }

    private void ShowAdLoadFailedMessage(string adType)
    {
        Debug.Log($"{adType} 광고 로드에 실패했습니다. 나중에 다시 시도해 주세요.");
    }

    #endregion
}