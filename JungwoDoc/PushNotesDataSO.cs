using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/PushNotesData")]
public class PushNotesDataSO : ScriptableObject
{
    [SerializeField] private string pushNoteID;
    [SerializeField] private string title_KR;
    [SerializeField] private string title_EN;
    [SerializeField] private string desc_KR;
    [SerializeField] private string desc_EN;
    [SerializeField] private int pushTime;
    [SerializeField] private bool useGrouping = true;

    public string PushNoteID => pushNoteID;
    public string Title_KR => title_KR;
    public string Title_EN => title_EN;
    public string Desc_KR => desc_KR;
    public string Desc_EN => desc_EN;
    public int PushTime => pushTime;
    public bool UseGrouping => useGrouping;
}
