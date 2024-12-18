using UnityEngine;
using System;
using System.Collections.Generic;
using JungWoo.Utilities;

#if UNITY_ANDROID
using UnityEngine.Android;
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class PushNotificationManager : MonoSingleton<PushNotificationManager>
{
    Dictionary<string, PushNotesDataSO> dataDic;
    public event Action OnApplicationPauseEvent;

    private bool isKorean;

    public int hasPermissionChecked
    {
        get { return ES3.Load<int>(Consts.PUSH_PERMISSION_CHECK_KEY + Application.productName, 0); }
        set { ES3.Save(Consts.PUSH_PERMISSION_CHECK_KEY + Application.productName, value, ES3.settings); }
    }
#if UNITY_EDITOR
    public bool GetIsPlayerStartInMainScene()
    {
        return dataDic == null;
    }
#endif

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
        // 게임시작 모든 알람 지우기
        AndroidNotificationCenter.CancelAllNotifications();
        AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif

        LoadDatas();
    }

    private void SetCollections()
    {
        dataDic = new Dictionary<string, PushNotesDataSO>();
    }

    private void LoadDatas()
    {
        // Addressable로 로드
        ResourceManager.Instance.LoadAsset<PushNotesDataSOList>(AddressableEnum.PushNotesDataSO, (datas) =>
        {
            foreach (PushNotesDataSO data in datas.pushNotesDataArray)
            {
                dataDic[data.name] = data;
                Debug.Log($"PushNotesDataSO loaded: {data.name}");
            }
            Debug.Log($"PushNotesDataSO loaded: {dataDic.Count}");
        },
        (error) =>
        {
            Debug.LogError($"Failed to load PushNotesDataSO: {error.Message}");
        });
    }

    private void CheckNotificationPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Consts.PUSH_NOTIFICATION_PERMISSION))
        {
            Permission.RequestUserPermission(Consts.PUSH_NOTIFICATION_PERMISSION);
        }
    }

#if UNITY_IOS
    public IEnumerator<string> RequestAuthorization()
    {
        using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            }
        }
    }
#endif

    private void OnApplicationPause(bool pause)
    {
#if UNITY_ANDROID
        if (pause)
        {
            // 알림 예약
            if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                OnApplicationPauseEvent?.Invoke();
                SendLocalNotification();
            }
        }
        else
        {
            // 알림 예약 제거
            AndroidNotificationCenter.CancelAllNotifications();
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }
#elif UNITY_IOS
        if (pause)
        {
            // 알림 예약
            OnApplicationPauseEvent?.Invoke();
            SendLocalNotification();
        }
        else
        {
            // 알림 예약 제거
            iOSNotificationCenter.RemoveAllScheduledNotifications();
        }
#endif
    }

    public void SendLocalNotification(string title, string desc, int minutes)
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Channel",
            Importance = Importance.Default,
            Description = "Description",
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        AndroidNotificationCenter.SendNotification(new AndroidNotification(title, desc, DateTime.Now.AddMinutes(minutes)), "channel_id");
#elif UNITY_IOS
        var notification = new iOSNotification()
        {
            Title = title,
            Body = desc,
            // Subtitle = "Subtitle",
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = new TimeSpan(0, minutes, 0),
                Repeats = false
            }
        };

        iOSNotificationCenter.ScheduleNotification(notification);

#endif
    }

    private void SendLocalNotification()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = Consts.PUSH_CHANNEL_ID,
            Name = Consts.PUSH_CHANNEL_NAME,
            Importance = Importance.Default,
            Description = Consts.PUSH_CHANNEL_DESC,
        };

        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        // 그룹화할 데이터와 개별 발송할 데이터를 분리
        Dictionary<int, List<PushNotesDataSO>> timeGroupedData = new Dictionary<int, List<PushNotesDataSO>>();
        foreach (KeyValuePair<string, PushNotesDataSO> kvp in dataDic)
        {
            if (kvp.Value.UseGrouping)
            {
                if (!timeGroupedData.ContainsKey(kvp.Value.PushTime))
                    timeGroupedData[kvp.Value.PushTime] = new List<PushNotesDataSO>();
                timeGroupedData[kvp.Value.PushTime].Add(kvp.Value);
            }
            else
            {
                // 그룹화하지 않는 알림은 바로 전송
                string title = isKorean ? kvp.Value.Title_KR : kvp.Value.Title_EN;
                string desc = isKorean ? kvp.Value.Desc_KR : kvp.Value.Desc_EN;

                Debug.Log($"Push Individual: {kvp.Value.name} at {kvp.Value.PushTime} hours");

                AndroidNotificationCenter.SendNotification(
                    new AndroidNotification(title, desc, DateTime.Now.AddHours(kvp.Value.PushTime)),
                    Consts.PUSH_CHANNEL_ID
                );
            }
        }

        // 그룹화된 알림 처리 (기존 로직)
        foreach (var timeGroup in timeGroupedData)
        {
            if (timeGroup.Value.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, timeGroup.Value.Count);
                var selectedData = timeGroup.Value[randomIndex];

                string title = isKorean ?
                    selectedData.Title_KR : selectedData.Title_EN;
                string desc = isKorean ?
                    selectedData.Desc_KR : selectedData.Desc_EN;

                Debug.Log($"Push Selected: {selectedData.name} at {timeGroup.Key} hours");

                AndroidNotificationCenter.SendNotification(
                    new AndroidNotification(title, desc, DateTime.Now.AddHours(timeGroup.Key)),
                    Consts.PUSH_CHANNEL_ID
                );
            }
        }

#elif UNITY_IOS          
    // iOS에서만 사용되는 푸시 채널 설정
    
    Dictionary<int, List<PushNotesDataSO>> timeGroupedData = new Dictionary<int, List<PushNotesDataSO>>();
    foreach (KeyValuePair<string, PushNotesDataSO> kvp in dataDic)
    {
        if (kvp.Value.UseGrouping)
        {
            if (!timeGroupedData.ContainsKey(kvp.Value.PushTime))
                timeGroupedData[kvp.Value.PushTime] = new List<PushNotesDataSO>();
            timeGroupedData[kvp.Value.PushTime].Add(kvp.Value);
        }
        else
        {
            string title = isKorean ? kvp.Value.Title_KR : kvp.Value.Title_EN;
            string desc = isKorean ? kvp.Value.Desc_KR : kvp.Value.Desc_EN;

            Debug.Log($"Push Individual: {kvp.Value.name} at {kvp.Value.PushTime} hours");

            var notification = new iOSNotification()
            {
                Title = title,
                Body = desc,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = new iOSNotificationTimeIntervalTrigger()
                {
                    TimeInterval = new TimeSpan(kvp.Value.PushTime, 0, 0),
                    Repeats = false
                }
            };

            iOSNotificationCenter.ScheduleNotification(notification);
        }
    }

    // 그룹화된 알림 처리 (기존 로직)
    foreach (var timeGroup in timeGroupedData)
    {
        if (timeGroup.Value.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, timeGroup.Value.Count);
            var selectedData = timeGroup.Value[randomIndex];
            
            string title = isKorean ? 
                selectedData.Title_KR : selectedData.Title_EN;
            string desc = isKorean ? 
                selectedData.Desc_KR : selectedData.Desc_EN;
            
            Debug.Log($"Push Selected: {selectedData.name} at {timeGroup.Key} hours");

            var notification = new iOSNotification()
            {
                Title = title,
                Body = desc,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = new iOSNotificationTimeIntervalTrigger()
                {
                    TimeInterval = new TimeSpan(timeGroup.Key, 0, 0),
                    Repeats = false
                }
            };

            iOSNotificationCenter.ScheduleNotification(notification);
        }
    }
#endif
    }

    private void SaveRewardRecieved()
    {
        // if (rewardRecieved == null) Initialize();

        // foreach (KeyValuePair<string, bool> kvp in rewardRecieved)
        // {
        //     ES3.Save($"{Consts.PUSH_REWARD_RECEIVED_KEY}{kvp.Key}", kvp.Value);
        // }
    }

    private void LoadRewardRecieved()
    {
        foreach (KeyValuePair<string, PushNotesDataSO> kvp in dataDic)
        {
            // rewardRecieved[kvp.Key] = ES3.KeyExists($"{Consts.PUSH_REWARD_RECEIVED_KEY}{kvp.Key}") ? 
            //     ES3.Load<bool>($"{Consts.PUSH_REWARD_RECEIVED_KEY}{kvp.Key}") : false;
        }
    }
}
