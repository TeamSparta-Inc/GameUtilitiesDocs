using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace JungWoo.Utilities
{
    public class GoogleSheetEnumGenerator : EditorWindow
    {
        private enum Mode { Public, Private }
        private Mode selectedMode = Mode.Public;

        private string spreadsheetUrl = "";
        private string credentialsFilePath = "";
        private string sheetName = "";
        private string keyColumn = "key";
        private string enumName = "AddressableEnum";
        private string outputPath = "Assets/AddressableEnum.cs";

        [MenuItem("Tools/Google Sheet Enum Generator")]
        public static void ShowWindow()
        {
            GetWindow<GoogleSheetEnumGenerator>("Google Sheet Enum Generator");
        }

        void OnGUI()
        {
            GUILayout.Label("Google Sheet Enum Generator", EditorStyles.boldLabel);

            selectedMode = (Mode)EditorGUILayout.EnumPopup("Mode", selectedMode);

            spreadsheetUrl = EditorGUILayout.TextField("Google Spreadsheet URL", spreadsheetUrl);

            if (selectedMode == Mode.Private)
            {
                credentialsFilePath = EditorGUILayout.TextField("Credentials File Path", credentialsFilePath);
                sheetName = EditorGUILayout.TextField("Sheet Name", sheetName);
            }

            keyColumn = EditorGUILayout.TextField("Key Column", keyColumn);
            enumName = EditorGUILayout.TextField("Enum Name", enumName);
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);

            if (GUILayout.Button("Generate Enum"))
            {
                GenerateEnum();
            }
        }

        void GenerateEnum()
        {
            List<string> keys = null;

            if (selectedMode == Mode.Public)
            {
                string csvUrl = ConvertToCsvUrl(spreadsheetUrl);
                if (!string.IsNullOrEmpty(csvUrl))
                {
                    keys = FetchFromPublicSheet(csvUrl);
                }
            }
            else
            {
                keys = FetchFromPrivateSheet(credentialsFilePath, spreadsheetUrl, sheetName);
            }

            if (keys != null && keys.Count > 0)
            {
                SaveEnumToFile(keys, enumName, outputPath);
                AssetDatabase.Refresh();
                Debug.Log($"Enum 생성 완료: {outputPath}");
            }
            else
            {
                Debug.LogError("키를 가져오지 못했습니다.");
            }
        }

        string ConvertToCsvUrl(string url)
        {
            try
            {
                var match = Regex.Match(url, @"https:\/\/docs\.google\.com\/spreadsheets\/d\/([a-zA-Z0-9-_]+)(\/edit#gid=([0-9]+))?");
                if (match.Success)
                {
                    string spreadsheetId = match.Groups[1].Value;
                    string gid = match.Groups[3].Success ? match.Groups[3].Value : "0";
                    string csvUrl = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=csv&gid={gid}";
                    Debug.Log($"CSV 링크로 변환된 URL: {csvUrl}");
                    return csvUrl;
                }
                else
                {
                    Debug.LogError("올바른 Google 스프레드시트 URL이 아닙니다.");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CSV URL 변환 중 오류가 발생했습니다: {e.Message}");
                return null;
            }
        }

        List<string> FetchFromPublicSheet(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string data = client.DownloadString(url);
                    var lines = data.Split('\n');
                    List<string> keys = new List<string>();

                    foreach (var line in lines)
                    {
                        var columns = line.Split(',');
                        if (columns.Length > 0)
                        {
                            keys.Add(columns[0].Trim());
                        }
                    }

                    return keys;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"공개 스프레드시트에서 데이터를 가져오는 중 오류가 발생했습니다: {e.Message}");
                return null;
            }
        }

        List<string> FetchFromPrivateSheet(string credentialsPath, string spreadsheetUrl, string sheetName)
        {
            Debug.LogError("비공개 스프레드시트 지원은 아직 구현되지 않았습니다.");
            return null;
        }

        void SaveEnumToFile(List<string> keys, string enumName, string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine($"public enum {enumName}");
                writer.WriteLine("{");
                foreach (var key in keys)
                {
                    writer.WriteLine($"    {key},");
                }
                writer.WriteLine("}");
            }
        }
    }
}