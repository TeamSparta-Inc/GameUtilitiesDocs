using UnityEngine;

/// <summary>
/// 여러 PushNotesDataSO를 담는 컨테이너 ScriptableObject
/// </summary>
[CreateAssetMenu(menuName = "Data/PushNotesDataSOList", fileName = "NewPushNotesDataSOList")]
public class PushNotesDataSOList : ScriptableObject
{
    [Header("Push Notes Data List")]
    [Tooltip("PushNotesDataSO를 담는 배열")]
    public PushNotesDataSO[] pushNotesDataArray;
}