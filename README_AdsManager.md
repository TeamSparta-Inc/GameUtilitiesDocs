# 📄 **README: AdsManager**

---

### 📌 **개요**

`AdsManager`는 Unity에서 광고를 관리하는 유틸리티 클래스입니다. **Firebase Analytics**와 **AppLovin MAX SDK**를 사용하여 보상형 광고 및 전면 광고를 표시하고, **일일 광고 노출 제한**을 설정합니다.

---

### 🚀 **기능**

1. **보상형 광고 및 전면 광고**  
   보상형 광고와 전면 광고를 초기화하고 표시합니다.

2. **일일 광고 노출 제한**  
   광고별로 일일 노출 횟수를 설정하고 초과 시 광고를 차단합니다.

3. **Firebase Analytics 통합**  
   광고 노출 및 사용자 상호작용을 Firebase Analytics에 기록합니다.

4. **자동 재시도 로직**  
   보상형 광고 로드 실패 시 자동으로 재시도합니다.

---

### 📊 **일일 광고 노출 제한 사용법**

#### **1. AdLimitConfig 설정**

`AdLimitConfig` 클래스를 사용해 각 광고에 대한 일일 노출 제한을 설정합니다.

```csharp
[Serializable]
public class AdLimitConfig
{
    public string adKey;     // 광고 키 (예: "rewarded_ad", "interstitial_ad")
    public int dailyLimit;   // 일일 노출 제한 횟수
}
```

#### **2. 노출 제한 설정 예제**

`AdsManager` 스크립트에서 일일 노출 제한을 설정합니다.

```csharp
[Header("Ad Limits")]
[Tooltip("광고별 일일 노출 제한 설정")]
[SerializeField] private List<AdLimitConfig> adLimitConfigs = new List<AdLimitConfig>
{
    new AdLimitConfig { adKey = "rewarded_ad", dailyLimit = 3 },
    new AdLimitConfig { adKey = "interstitial_ad", dailyLimit = 5 }
};
```

#### **3. 광고 노출 제한 확인**

광고를 표시하기 전에 해당 광고의 일일 노출 횟수를 확인하고 초과하면 광고를 차단합니다.

```csharp
private bool CanShowAd(string adKey)
{
    if (!adsShownToday.ContainsKey(adKey))
    {
        adsShownToday[adKey] = 0;
    }

    return adsShownToday[adKey] < GetDailyLimit(adKey);
}

private int GetDailyLimit(string adKey)
{
    var config = adLimitConfigs.Find(c => c.adKey == adKey);
    return config != null ? config.dailyLimit : int.MaxValue;
}
```

#### **4. 광고 보기 예제**

광고를 보기 전에 `CanShowAd` 메서드로 확인합니다.

```csharp
public void ShowRewardedAd(Action<MaxSdk.Reward, MaxSdkBase.AdInfo> rewardAction)
{
    if (!CanShowAd("rewarded_ad"))
    {
        Debug.Log("Daily limit for rewarded ads reached.");
        return;
    }

    if (MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
    {
        pendingRewardAction = rewardAction;
        MaxSdk.ShowRewardedAd(rewardedAdUnitId);
        adsShownToday["rewarded_ad"]++;
        SaveAdData();
    }
    else
    {
        Debug.Log("Rewarded ad not ready.");
        LoadRewardedAd();
    }
}
```

#### **5. 데이터 저장 및 불러오기**

일일 노출 횟수를 저장하고 불러옵니다.

```csharp
private void SaveAdData()
{
    ES3.Save(Consts.ADS_SHOWN_TODAY_SAVE, adsShownToday);
    ES3.Save(Consts.LAST_AD_SHOW_DATE_SAVE, DateTime.Today);
}

private void LoadAdData()
{
    adsShownToday = ES3.Load(Consts.ADS_SHOWN_TODAY_SAVE, new Dictionary<string, int>());
    lastAdShownDate = ES3.Load(Consts.LAST_AD_SHOW_DATE_SAVE, DateTime.Today);

    if (adsShownToday == null)
    {
        adsShownToday = new Dictionary<string, int>();
    }

    // 날짜가 변경되면 데이터 초기화
    if (lastAdShownDate != DateTime.Today)
    {
        adsShownToday.Clear();
        lastAdShownDate = DateTime.Today;
        SaveAdData();
    }
}
```

---

### ⚠️ **주의사항**

1. **일일 노출 제한 데이터**  
   ES3를 사용하여 광고 노출 횟수를 저장하고 날짜가 변경되면 데이터가 초기화됩니다.

2. **AdLimitConfig**  
   모든 광고 키에 대해 적절한 일일 노출 제한을 설정하세요.

3. **AppLovin MAX SDK**  
   광고를 표시하려면 AppLovin MAX SDK가 통합되어 있어야 합니다.

4. **Firebase Analytics**  
   Firebase Analytics를 통해 광고 이벤트를 추적할 수 있습니다.

---

### 📚 **확장하기**

- **광고 유형 추가**: 배너 광고나 네이티브 광고를 추가할 수 있습니다.
- **사용자 경험 개선**: 광고가 준비되지 않았을 때 사용자에게 알림을 표시합니다.
- **데이터 리셋 기능**: 일일 광고 노출 제한을 리셋하는 기능을 추가할 수 있습니다.

---

이 스크립트를 활용하여 Unity 프로젝트에서 **효율적으로 광고를 관리**하고 사용자 경험을 향상시키세요! 📢😊