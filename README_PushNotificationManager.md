# 📄 **README: PushNotificationManager**

---

### 📌 **개요**

`PushNotificationManager`는 **Unity의 로컬 푸시 알림 시스템**을 다루기 위한 모듈입니다. 이 스크립트는 플랫폼(Android, iOS)에 따라 적절한 푸시 알림을 설정하고 관리합니다. **Addressable 시스템**을 통해 알림 데이터를 로드하고, 알림 권한을 관리합니다.

---

### 🚀 **기능**

1. **로컬 푸시 알림 지원**  
   Android와 iOS에서 로컬 푸시 알림을 지원합니다.

2. **Addressable 데이터 로딩**  
   `PushNotesDataSO` 데이터를 Addressable 시스템을 통해 로드합니다.

3. **권한 체크**  
   최초 실행 시 푸시 알림 권한을 요청하고 확인합니다.

4. **알림 초기화 및 관리**  
   앱 시작 시 모든 예약된 알림을 취소하고 새로 설정합니다.

5. **다국어 지원**  
   시스템 언어를 확인하여 한국어 여부를 판단합니다.

---

### 🛠️ **주요 코드 예제**

#### **1. 초기화 및 권한 체크**

`Start` 메서드에서 초기화를 시작하며, 한 번만 푸시 알림 권한을 체크합니다.

```csharp
private void Start()
{
    StartInit();
}

public void StartInit()
{
    DontDestroyOnLoad(gameObject);
    Initialize();
}

private void Initialize()
{
    SetCollections();

    isKorean = Application.systemLanguage == SystemLanguage.Korean;

    // 최초 한 번만 권한 체크
    if (hasPermissionChecked == 0)
    {
        CheckNotificationPermission();
        hasPermissionChecked = 1;
    }

    #if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
        AndroidNotificationCenter.CancelAllScheduledNotifications();
    #elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
    #endif

    LoadDatas();
}
```

#### **2. Addressable을 이용한 데이터 로딩**

`LoadDatas` 메서드를 통해 `PushNotesDataSOList`를 Addressable에서 로드합니다.

```csharp
private void LoadDatas()
{
    ResourceManager.Instance.LoadAsset<PushNotesDataSOList>(AddressableEnum.PushNotesDataSO, (datas) =>
    {
        foreach (PushNotesDataSO data in datas.pushNotesDataArray)
        {
            dataDic[data.name] = data;
            Debug.Log($"Loaded Push Notification Data: {data.name}");
        }
    });
}
```

#### **3. 권한 체크**

Android 및 iOS에서 푸시 알림 권한을 확인하고 요청합니다.

```csharp
private void CheckNotificationPermission()
{
    #if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    #elif UNITY_IOS
        iOSNotificationCenter.RequestAuthorization((granted, error) =>
        {
            Debug.Log($"Push Notification Permission: {granted}");
        });
    #endif
}
```

---

### 📦 **관련 데이터 클래스**

#### **PushNotesDataSO.cs**

```csharp
[CreateAssetMenu(fileName = "PushNotesData", menuName = "Push Notifications/PushNotesData")]
public class PushNotesDataSO : ScriptableObject
{
    public string title;
    public string message;
    public int delaySeconds;
}
```

#### **PushNotesDataSOList.cs**

```csharp
[CreateAssetMenu(fileName = "PushNotesDataList", menuName = "Push Notifications/PushNotesDataList")]
public class PushNotesDataSOList : ScriptableObject
{
    public PushNotesDataSO[] pushNotesDataArray;
}
```

---

### ⚠️ **주의사항**

1. **Addressable 키**가 `AddressableEnum`에 정의되어 있어야 합니다.
2. **권한 체크**는 플랫폼에 따라 다르게 작동합니다(Android와 iOS).
3. **푸시 알림 데이터**는 `PushNotesDataSO`와 `PushNotesDataSOList`에서 관리됩니다.

---

### 📚 **확장하기**

- **알림 예약**: 알림을 특정 시간에 예약하고 싶다면 `AndroidNotificationCenter.SendNotification`이나 `iOSNotificationCenter.ScheduleNotification`을 활용하세요.
- **다양한 언어 지원**: `isKorean` 변수를 확장하여 여러 언어를 지원할 수 있습니다.

---

이 모듈을 활용하여 사용자에게 중요한 알림을 효과적으로 전달하세요! 😊