using Google.GData.Client;
using Google.GData.Spreadsheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class GoogleSheetsImporter : EditorWindow
{
    static GoogleSheetsImporter m_Instance;

    public static GoogleSheetsImporter instance
    {
        get
        {
            if (m_Instance == null)
            {
                Init();
            }

            return m_Instance;
        }
    }

    #region Variables
    OAuth2Parameters m_OAuth2Parameters;
    OAuth2Data m_OAuth2Data;
    string m_AccessCode;
    string m_ScriptFilePath;
    string m_ScriptableObjectFilePath;
    string m_XmlFilePath;
    List<SpreadsheetEntry> m_SpreadsheetEntryList = new List<SpreadsheetEntry>();
    List<ScriptData> m_ScriptDataList = new List<ScriptData>();
    Vector2 m_SpreadsheetEntryListScrollPosition;
    Vector2 m_ScriptDataListScrollPosition;
    Vector2 m_WorksheetDataListScrollPosition;
    StringBuilder m_SB = new StringBuilder();
    Vector2 m_SpreadsheetListScrollPosition;
    List<WorksheetData> m_WorksheetDataList = new List<WorksheetData>();
    string m_AssetBundleFilePath;
    LanguageCode m_LanguageCode = LanguageCode.Korean;
    #endregion

    #region Properties
    public SpreadsheetsService spreadsheetsService
    {
        get
        {
            return new SpreadsheetsService(string.Empty)
            {
                RequestFactory = GoogleSheetsSettings.instance.m_GOAuth2RequestFactory,
            };
        }
    }

    bool isAllScriptDataListSelected
    {
        get;
        set;
    }

    bool isPathModified
    {
        get;
        set;
    }

    static bool isAuthenticated
    {
        get;
        set;
    }

    bool isAllWorksheetDataListSelected
    {
        get;
        set;
    }

    bool isBuildAssetBundleStandaloneWindows
    {
        get;
        set;
    }

    bool isBuildAssetBundleAndroid
    {
        get;
        set;
    }

    bool isBuildAssetBundleEditor
    {
        get;
        set;
    }

    bool isBuildAssetBundleiOS
    {
        get;
        set;
    }
    #endregion

    [MenuItem("Google Sheets/Google Sheets Importer")]
    static void Init()
    {
        m_Instance = EditorWindow.GetWindow<GoogleSheetsImporter>();
        m_Instance.titleContent.text = "Google Sheets Importer";

        UnsafeSecurityPolicy.Instate();
    }

    void OnEnable()
    {
        RefreshScriptDataList();
        m_OAuth2Data = GoogleSheetsSettings.instance.m_OAuth2Data;
        m_ScriptFilePath = GoogleSheetsSettings.instance.m_ScriptFilePath;
        m_ScriptableObjectFilePath = GoogleSheetsSettings.instance.m_ScriptableObjectFilePath;
        m_XmlFilePath = GoogleSheetsSettings.instance.m_XmlFilePath;
        m_AssetBundleFilePath = GoogleSheetsSettings.instance.m_AssetBundleFilePath;
        m_AccessCode = string.Empty;
    }

    void OnGUI()
    {
        GUILayout.Space(10);

        DrawAuthenticate();

        GUILayout.Space(10);

        DrawSpreadsheetList();

        GUILayout.Space(10);

        DrawWorksheetList();

        GUILayout.Space(10);

        DrawScriptList();

        GUILayout.Space(10);

        DrawGenerateScriptableObjectAndXml();

        GUILayout.Space(10);

        DrawPath();

        GUILayout.Space(10);

        DrawAssetBundle();
    }

    #region
    void DrawSpreadsheetList()
    {
        GUI.enabled = isAuthenticated;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spreadsheet List");
        if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
        {
            RefreshSpreadsheetEntryList();
            Repaint();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        m_SpreadsheetListScrollPosition = EditorGUILayout.BeginScrollView(
            m_SpreadsheetListScrollPosition,
            false,
            true,
            GUILayout.Width(position.width),
            GUILayout.MaxHeight(128));
        if (m_SpreadsheetEntryList != null
            && m_SpreadsheetEntryList.Count > 0)
        {
            if (GUILayout.Button("All", GUILayout.ExpandWidth(false)))
            {
                m_SelectedSpreadsheetEntryList.Clear();
                m_SelectedSpreadsheetEntryList.AddRange(m_SpreadsheetEntryList);
                RefreshWorksheetDataList();
            }
            for (int i = 0; i < m_SpreadsheetEntryList.Count; i++)
            {
                SpreadsheetEntry spreadsheetEntry = m_SpreadsheetEntryList[i];
                if (spreadsheetEntry != null)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(spreadsheetEntry.Title.Text, GUILayout.ExpandWidth(false)))
                    {
                        m_SelectedSpreadsheetEntryList.Clear();
                        m_SelectedSpreadsheetEntryList.Add(spreadsheetEntry);
                        RefreshWorksheetDataList();
                    }
                    GUILayout.Space(10);
                    GUILayout.Label(string.Format("(Updated at {0})", spreadsheetEntry.Updated.ToString("yyyy/MM/dd hh:mm:ss"), GUILayout.ExpandWidth(false)));
                    GUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    Vector2 m_WorksheetListScrollPosition;
    List<SpreadsheetEntry> m_SelectedSpreadsheetEntryList = new List<SpreadsheetEntry>();
    List<string> m_GeneratedFileNameList = new List<string>();

    void DrawWorksheetList()
    {
        GUI.enabled = isAuthenticated;
        GUILayout.BeginHorizontal();
        bool isAllWorksheetListSelected = GUILayout.Toggle(this.isAllWorksheetDataListSelected, string.Empty, GUILayout.ExpandWidth(false));
        if (this.isAllWorksheetDataListSelected != isAllWorksheetListSelected)
        {
            this.isAllWorksheetDataListSelected = isAllWorksheetListSelected;
            for (int i = 0; i < m_WorksheetDataList.Count; i++)
            {
                WorksheetData worksheetData = m_WorksheetDataList[i];
                if (worksheetData != null)
                {
                    worksheetData.generate = this.isAllWorksheetDataListSelected;
                }
            }
        }
        GUILayout.Label("Worksheet List");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        m_WorksheetListScrollPosition = EditorGUILayout.BeginScrollView(
            m_WorksheetListScrollPosition,
            false,
            true,
            GUILayout.Width(position.width),
            GUILayout.Height(128));
        if (m_WorksheetDataList != null
            && m_WorksheetDataList.Count > 0)
        {
            for (int i = 0; i < m_WorksheetDataList.Count; i++)
            {
                WorksheetData worksheetData = m_WorksheetDataList[i];
                if (worksheetData != null)
                {
                    GUILayout.BeginHorizontal();
                    worksheetData.generate = GUILayout.Toggle(
                        worksheetData.generate,
                        worksheetData.worksheetFullName,
                        GUILayout.ExpandWidth(false));
                    GUILayout.Space(10);
                    if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false))
                        && WorksheetDataEditor.instance != null)
                    {
                        WorksheetDataEditor.instance.SetWorksheetData(worksheetData);
                    }
                    GUILayout.Space(10);
                    GUILayout.Label(string.Format("(Updated at {0})", worksheetData.m_WorksheetEntry.Updated.ToString("yyyy/MM/dd hh:mm:ss")), GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                }
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.Space(10);
        //GUI.enabled = false;
        if (GUILayout.Button("Generate *.cs and *ScriptableObject.cs", GUILayout.ExpandWidth(false)))
        {
            if (m_WorksheetDataList != null
                && m_WorksheetDataList.Count > 0)
            {
                Utility.DisplayProgressBar(string.Empty, string.Empty, 0, m_WorksheetDataList.Count);
                string filePath = Utility.CompleteFilePath(GoogleSheetsSettings.instance.m_ScriptFilePath, true);
                m_GeneratedFileNameList.Clear();
                for (int i = 0; i < m_WorksheetDataList.Count; i++)
                {
                    WorksheetData worksheetData = m_WorksheetDataList[i];
                    if (worksheetData != null)
                    {
                        Utility.DisplayProgressBar(worksheetData.worksheetFullName, string.Empty, i + 1, m_WorksheetDataList.Count);
                        if (worksheetData.generate)
                        {
                            if (!worksheetData.initialized)
                            {
                                worksheetData.CellQuery();
                            }

                            string fileName = string.Empty;
                            if (worksheetData.isEnumWorksheet)
                            {
                                // Enum.cs 생성
                                fileName = "Enum.cs";
                                Writer.WriteFile(filePath + fileName, Writer.WriteEnumCS(worksheetData));
                                m_GeneratedFileNameList.Add(fileName);
                            }
                            else
                            {
                                // *.cs 생성
                                fileName = worksheetData.worksheetName + ".cs";
                                Writer.WriteFile(filePath + fileName, Writer.WriteTableObjectCS(worksheetData));
                                m_GeneratedFileNameList.Add(fileName);

                                // *ScriptableObject.cs 생성
                                fileName = worksheetData.worksheetName + "ScriptableObject.cs";
                                Writer.WriteFile(filePath + fileName, Writer.WriteScriptableObjectCS(worksheetData));
                                m_GeneratedFileNameList.Add(fileName);
                            }
                        }
                    }
                }
                if (m_GeneratedFileNameList != null
                    && m_GeneratedFileNameList.Count > 0)
                {
                    m_SB.Remove(0, m_SB.Length);
                    m_SB.AppendLine(filePath);
                    m_SB.AppendLine();
                    for (int i = 0; i < m_GeneratedFileNameList.Count; i++)
                    {
                        m_SB.AppendLine(m_GeneratedFileNameList[i]);
                    }

                    EditorUtility.DisplayDialog(string.Empty, m_SB.ToString(), "OK");
                    AssetDatabase.Refresh();
                }
                Utility.ClearProgressBar();
            }
        }
    }

    void DrawScriptList()
    {
        GUI.enabled = isAuthenticated;
        GUILayout.BeginHorizontal();
        bool isAllScriptDataListSelected = GUILayout.Toggle(this.isAllScriptDataListSelected, string.Empty, GUILayout.ExpandWidth(false));
        if (this.isAllScriptDataListSelected != isAllScriptDataListSelected)
        {
            this.isAllScriptDataListSelected = isAllScriptDataListSelected;
            for (int i = 0; i < m_ScriptDataList.Count; i++)
            {
                ScriptData scriptData = m_ScriptDataList[i];
                if (scriptData != null)
                {
                    scriptData.m_Generate = this.isAllScriptDataListSelected;
                }
            }
        }
        GUILayout.Label("Script List");
        if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
        {
            RefreshScriptDataList();
        }
        GUILayout.EndHorizontal();
        if (m_ScriptDataList != null
            && m_ScriptDataList.Count > 0)
        {
            GUILayout.Space(10);
            m_ScriptDataListScrollPosition = EditorGUILayout.BeginScrollView(
                m_ScriptDataListScrollPosition,
                false,
                true,
                GUILayout.Width(position.width),
                GUILayout.Height(128));
            for (int i = 0; i < m_ScriptDataList.Count; i++)
            {
                ScriptData scriptData = m_ScriptDataList[i];
                if (scriptData != null)
                {
                    scriptData.m_Generate = GUILayout.Toggle(scriptData.m_Generate, scriptData.m_Name, GUILayout.ExpandWidth(false));
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
    #endregion

    void DrawAssetBundle()
    {
        GUI.enabled = true;
        GUILayout.BeginHorizontal();
        GUILayout.Label("AssetBundle", GUILayout.Width(100));
        m_AssetBundleFilePath = EditorGUILayout.TextField(m_AssetBundleFilePath);
        GUILayout.EndHorizontal();

        if (GoogleSheetsSettings.instance.m_AssetBundleFilePath != m_AssetBundleFilePath)
        {
            GoogleSheetsSettings.instance.m_AssetBundleFilePath = m_AssetBundleFilePath;
            GoogleSheetsSettings.instance.Save();
        }

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        isBuildAssetBundleEditor = GUILayout.Toggle(isBuildAssetBundleEditor, "Editor", GUILayout.ExpandWidth(false));
        isBuildAssetBundleStandaloneWindows = GUILayout.Toggle(isBuildAssetBundleStandaloneWindows, "Standalone Windows", GUILayout.ExpandWidth(false));
        isBuildAssetBundleAndroid = GUILayout.Toggle(isBuildAssetBundleAndroid, "Android", GUILayout.ExpandWidth(false));
        isBuildAssetBundleiOS = GUILayout.Toggle(isBuildAssetBundleiOS, "iOS", GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        //GUI.enabled = GUI.enabled && (isBuildAssetBundleEditor || isBuildAssetBundleStandaloneWindows || isBuildAssetBundleAndroid);

        if (GUILayout.Button("Generate AssetBundle", GUILayout.ExpandWidth(false)))
        {
            if (string.IsNullOrEmpty(m_AssetBundleFilePath))
            {
                EditorUtility.DisplayDialog(string.Empty, string.Empty, string.Empty); //
            }
            else
            {
                if (isBuildAssetBundleEditor)
                {
                    GenerateAssetBundle(m_AssetBundleFilePath);
                }

                if (isBuildAssetBundleStandaloneWindows)
                {
                    GenerateAssetBundle(m_AssetBundleFilePath, (int)BuildTarget.StandaloneWindows);
                }

                if (isBuildAssetBundleAndroid)
                {
                    GenerateAssetBundle(m_AssetBundleFilePath, (int)BuildTarget.Android);
                }

                if (isBuildAssetBundleiOS)
                {
                    GenerateAssetBundle(m_AssetBundleFilePath, (int)BuildTarget.iOS);
                }

                m_SB.Remove(0, m_SB.Length);
                if (isBuildAssetBundleEditor)
                {
                    m_SB.AppendFormat("{0}Editor/", m_AssetBundleFilePath);
                    m_SB.AppendLine();
                }
                if (isBuildAssetBundleStandaloneWindows)
                {
                    m_SB.AppendFormat("{0}StandaloneWindows/", m_AssetBundleFilePath);
                    m_SB.AppendLine();
                }
                if (isBuildAssetBundleAndroid)
                {
                    m_SB.AppendFormat("{0}Android/", m_AssetBundleFilePath);
                    m_SB.AppendLine();
                }
                if (isBuildAssetBundleiOS)
                {
                    m_SB.AppendFormat("{0}iOS/", m_AssetBundleFilePath);
                    m_SB.AppendLine();
                }

                EditorUtility.DisplayDialog(string.Empty, m_SB.ToString(), "OK");
            }
        }

        GUI.enabled = GUI.enabled;
    }

    void GenerateAssetBundle(string filePath, int buildTarget = 0)
    {
        // If buildTarget is 0, build assetbundle for editor.
        if (!string.IsNullOrEmpty(filePath))
        {
            filePath = Utility.CompleteFilePath(filePath, false);
            if (buildTarget != 0)
            {
                filePath = string.Format("{0}{1}", filePath, (BuildTarget)buildTarget);
            }
            else
            {
                filePath = filePath + "Editor";
            }
            filePath = Utility.CompleteFilePath(filePath, true);

            if (buildTarget != 0)
            {
                BuildPipeline.BuildAssetBundles(filePath, BuildAssetBundleOptions.None, (BuildTarget)buildTarget);
            }
            else
            {
                BuildPipeline.BuildAssetBundles(filePath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            }

            EditorApplication.SaveAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 파일 이름 변경
            // * -> *.asset
            string[] files = Directory.GetFiles(filePath);
            for (int i = 0; i < files.Length; i++)
            {
                bool isAsset = false;
                string[] splitted = files[i].Split('/');
                string fileName = splitted[splitted.Length - 1];
                if (string.Equals(fileName, "gamedb"))
                {
                    isAsset = true;
                }
                else
                {
                    foreach (LanguageCode languageCode in Enum.GetValues(typeof(LanguageCode)))
                    {
                        if (string.Equals(fileName, "localdb_" + languageCode.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isAsset = true;
                            break;
                        }
                    }
                }

                if (isAsset)
                {
                    File.Move(files[i], files[i] + ".asset");
                }
                else
                {
                    File.Delete(files[i]);
                }
            }
        }
    }

    void DrawGenerateScriptableObjectAndXml()
    {
        GUI.enabled = isAuthenticated;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate *.asset and *.xml", GUILayout.ExpandWidth(false)))
        {
            GenerateScriptableObjectAndXml();
        }
        GUILayout.Space(10);
        GUILayout.Label("LanguageCode");
        m_LanguageCode = (LanguageCode)EditorGUILayout.EnumPopup(m_LanguageCode, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
    }

    void DrawPath()
    {
        GUI.enabled = isAuthenticated;
        isPathModified = false;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Script", GUILayout.Width(100));
        m_ScriptFilePath = EditorGUILayout.TextField(m_ScriptFilePath);
        GUILayout.EndHorizontal();
        if (GoogleSheetsSettings.instance.m_ScriptFilePath != m_ScriptFilePath)
        {
            GoogleSheetsSettings.instance.m_ScriptFilePath = m_ScriptFilePath;
            isPathModified = true;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("ScriptableObject", GUILayout.Width(100));
        m_ScriptableObjectFilePath = EditorGUILayout.TextField(m_ScriptableObjectFilePath);
        GUILayout.EndHorizontal();
        if (GoogleSheetsSettings.instance.m_ScriptableObjectFilePath != m_ScriptableObjectFilePath)
        {
            GoogleSheetsSettings.instance.m_ScriptableObjectFilePath = m_ScriptableObjectFilePath;
            isPathModified = true;
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Xml", GUILayout.Width(100));
        m_XmlFilePath = EditorGUILayout.TextField(m_XmlFilePath);
        GUILayout.EndHorizontal();
        if (GoogleSheetsSettings.instance.m_XmlFilePath != m_XmlFilePath)
        {
            GoogleSheetsSettings.instance.m_XmlFilePath = m_XmlFilePath;
            isPathModified = true;
        }
        if (isPathModified)
        {
            EditorUtility.SetDirty(GoogleSheetsSettings.instance);
            EditorApplication.SaveAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void DrawAuthenticate()
    {
        GUI.enabled = !isAuthenticated;

        #region OAuth2Data
        GUILayout.BeginHorizontal();

        string filePath = Utility.CompleteFilePath(GoogleSheetsSettings.instance.m_ClientSecretFilePath, false);
        filePath = GUILayout.TextField(filePath, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("...", GUILayout.ExpandWidth(false)))
        {
            filePath = EditorUtility.OpenFilePanel(string.Empty, Path.GetDirectoryName(filePath), "json");
            if (!string.IsNullOrEmpty(filePath))
            {
                m_SB.Remove(0, m_SB.Length);
                using (StreamReader streamReader = new StreamReader(filePath))
                {
                    string text = string.Empty;
                    while (text != null)
                    {
                        text = streamReader.ReadLine();
                        m_SB.Append(text);
                    }
                    streamReader.Close();
                }
                m_OAuth2Data = JsonConvert.DeserializeObject<OAuth2Data>(JObject.Parse(m_SB.ToString()).SelectToken("installed").ToString());
                //if (GoogleSheetsSettings.Instance.m_OAuth2Data != m_OAuth2Data)
                {
                    GoogleSheetsSettings.instance.m_OAuth2Data = m_OAuth2Data;
                    EditorUtility.SetDirty(GoogleSheetsSettings.instance);
                    EditorApplication.SaveAssets();
                    AssetDatabase.SaveAssets();
                }
            }
        }
        if (GoogleSheetsSettings.instance.m_ClientSecretFilePath != filePath)
        {
            GoogleSheetsSettings.instance.m_ClientSecretFilePath = filePath;
            EditorUtility.SetDirty(GoogleSheetsSettings.instance);
            EditorApplication.SaveAssets();
            AssetDatabase.SaveAssets();
        }

        GUILayout.EndHorizontal();
        #endregion

        GUILayout.Space(10);
        #region Access Code
        GUILayout.BeginHorizontal();
        GUILayout.Label("Access Code", GUILayout.ExpandWidth(false));
        m_AccessCode = EditorGUILayout.TextField(string.Empty, m_AccessCode, GUILayout.ExpandWidth(true));
        if (GoogleSheetsSettings.instance.m_AccessCode != m_AccessCode)
        {
            GoogleSheetsSettings.instance.m_AccessCode = m_AccessCode;
            EditorUtility.SetDirty(GoogleSheetsSettings.instance);
            EditorApplication.SaveAssets();
            AssetDatabase.SaveAssets();
        }
        GUILayout.EndHorizontal();
        #endregion
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        #region Start Authenticate
        if (GUILayout.Button("Start Authenticate", GUILayout.ExpandWidth(false)))
        {
            StartAuthenticate(
                m_OAuth2Data.client_id,
                m_OAuth2Data.client_secret,
                m_OAuth2Data.redirect_uris[0],
                "https://spreadsheets.google.com/feeds https://docs.google.com/feeds");
        }
        #endregion
        GUILayout.Space(10);
        #region Finish Authenticate
        if (GUILayout.Button("Finish Authenticate", GUILayout.ExpandWidth(false)))
        {
            if (FinishAuthenticate())
            {
                //RefreshWorksheetDataList();
                RefreshSpreadsheetEntryList();
            }
        }
        #endregion
        GUILayout.EndHorizontal();
    }

    void GenerateScriptableObjectAndXml()
    {
        if (m_ScriptDataList != null
            && m_ScriptDataList.Count > 0)
        {
            EditorUtility.DisplayProgressBar(string.Empty, string.Empty, 0);

            float value = 0, maxValue = m_ScriptDataList.Count;
            for (int i = 0; i < m_ScriptDataList.Count; i++)
            {
                value = i;
                ScriptData scriptData = m_ScriptDataList[i];
                if (scriptData != null
                    && scriptData.m_Generate)
                {
                    EditorUtility.DisplayProgressBar(scriptData.m_Name, string.Format("{0}/{1}", value, maxValue), (value / maxValue));
                    if (GenerateScriptableObjectAndXml(scriptData.m_Name))
                    {

                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }

    bool GenerateScriptableObjectAndXml(string scriptName)
    {
        try
        {
            // Localization
            string worksheetName = scriptName;
            if (worksheetName.StartsWith("DBStr_"/*, StringComparison.OrdinalIgnoreCase*/) &&
                m_LanguageCode != LanguageCode.Unknown)
            {
                // 임시 처리
                if (!worksheetName.EndsWith("BuiltIn"))
                {
                    worksheetName = string.Format("{0}_{1}", worksheetName, m_LanguageCode);
                }
            }
            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(scriptName + "ScriptableObject");
            if (scriptableObject != null)
            {
                Type type = null;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var item in assemblies)
                {
                    type = item.GetType(scriptName);
                    if (type != null)
                    {
                        break;
                    }
                }
                FieldInfo[] fieldInfos = scriptableObject.GetType().GetFields();
                Type[] schemaTypes = type.GetNestedTypes();
                // 0 : Field, 1 : Schema
                FieldInfo[] schemaFieldInfos = schemaTypes[1].GetFields();
                //string spreadsheetName = FindSpreadsheetName(scriptName);
                WorksheetEntry worksheetEntry = FindWorksheetEntry(worksheetName);
                if (worksheetEntry != null)
                {
                    SpreadsheetsService spreadsheetsService = this.spreadsheetsService;
                    CellQuery cellQuery = new CellQuery(worksheetEntry.CellFeedLink);
                    CellFeed cellFeed = spreadsheetsService.Query(cellQuery);
                    List<object> schemas = new List<object>();
                    float value = 0, maxValue = cellFeed.Entries.Count;
                    for (int i = 0; i < cellFeed.Entries.Count; i++)
                    {
                        value = i + 1;
                        EditorUtility.DisplayProgressBar(worksheetName, string.Format("{0}/{1}", value, maxValue), (value / maxValue));
                        CellEntry cellEntry = cellFeed.Entries[i] as CellEntry;
                        if (cellEntry.Row > 2)
                        {
                            for (int j = 0; j < schemaFieldInfos.Length; j++)
                            {
                                if (Equals(cellFeed[2, cellEntry.Column].Value, schemaFieldInfos[j].Name))
                                {
                                    int index = (int)cellEntry.Row - 2;
                                    bool isNew = schemas.Count < index;
                                    object schema = isNew ? Activator.CreateInstance(schemaTypes[1]) : schemas[index - 1];
                                    FieldInfo fieldInfo = schema.GetType().GetField(schemaFieldInfos[j].Name);
                                    if (fieldInfo.FieldType == typeof(bool))
                                    {
                                        bool val;

                                        if (bool.TryParse(cellEntry.NumericValue, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                        else if (bool.TryParse(cellEntry.InputValue, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                        else if (bool.TryParse(cellEntry.Value, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                    }
                                    if (fieldInfo.FieldType == typeof(int))
                                    {
                                        int val;

                                        if (int.TryParse(cellEntry.NumericValue, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                        else if (int.TryParse(cellEntry.InputValue, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                        else if (int.TryParse(cellEntry.Value, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }

                                    }
                                    else if (fieldInfo.FieldType == typeof(float))
                                    {
                                        float val;

                                        if (float.TryParse(cellEntry.NumericValue, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                        else if (float.TryParse(cellEntry.InputValue, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                        else if (float.TryParse(cellEntry.Value, out val))
                                        {
                                            fieldInfo.SetValue(schema, val);
                                        }
                                    }
                                    else if (fieldInfo.FieldType == typeof(string))
                                    {
                                        string inputValue = cellEntry.InputValue;
                                        if (!string.IsNullOrEmpty(inputValue))
                                        {
                                            inputValue = inputValue.Replace("\\n", "\n");
                                        }

                                        fieldInfo.SetValue(schema, inputValue);
                                    }
                                    else if (fieldInfo.FieldType.BaseType == typeof(Enum))
                                    {
                                        Array values = Enum.GetValues(fieldInfo.FieldType);
                                        foreach (var item in values)
                                        {
                                            string name = Enum.GetName(fieldInfo.FieldType, item);
                                            if (string.Equals(name, cellEntry.InputValue))
                                            {
                                                fieldInfo.SetValue(schema, item);
                                            }
                                        }
                                    }

                                    if (isNew)
                                    {
                                        schemas.Add(schema);
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 0; i < fieldInfos.Length; i++)
                    {
                        if (Equals("m_SchemaList", fieldInfos[i].Name))
                        {
                            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(schemas[0].GetType()));
                            foreach (var schema in schemas)
                            {
                                list.Add(schema);
                            }
                            fieldInfos[i].SetValue(scriptableObject, list);
                        }
                    }

                    return GenerateAsset(GoogleSheetsSettings.instance.m_ScriptableObjectFilePath,
                                         worksheetName,
                                         scriptableObject)
                           && GenerateXml(GoogleSheetsSettings.instance.m_XmlFilePath,
                                          worksheetName,
                                          scriptableObject);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(scriptName);
            Debug.Log(e);
        }

        return false;
    }

    bool GenerateXml(string filePath, string fileName, ScriptableObject scriptableObject)
    {
        if (!string.IsNullOrEmpty(filePath)
            && scriptableObject != null)
        {
            filePath = Utility.CompleteFilePath(filePath, true);
            filePath = string.Format("{0}{1}ScriptableObject.xml", filePath, fileName);

            FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
            XmlSerializer xmlSerializer = new XmlSerializer(scriptableObject.GetType());
            xmlSerializer.Serialize(streamWriter, scriptableObject);
            fileStream.Close();
            streamWriter.Close();
        }

        return false;
    }

    bool GenerateAsset(string filePath, string fileName, ScriptableObject scriptableObject)
    {
        if (!string.IsNullOrEmpty(filePath)
            && scriptableObject != null)
        {
            filePath = Utility.CompleteFilePath(filePath, true);
            filePath = string.Format("{0}{1}ScriptableObject.asset", filePath, fileName);

            AssetDatabase.CreateAsset(scriptableObject, filePath);
            // Localization
            AssetImporter assetImporter = AssetImporter.GetAtPath(filePath);
            if (assetImporter != null)
            {
                string assetBundleName = string.Empty;
                if (fileName.StartsWith("DB_"))
                {
                    assetBundleName = "gamedb";
                }
                else if (fileName.StartsWith("DBStr_"))
                {
                    foreach (LanguageCode languageCode in Enum.GetValues(typeof(LanguageCode)))
                    {
                        if (fileName.EndsWith(languageCode.ToString()/*, StringComparison.OrdinalIgnoreCase*/))
                        {
                            assetBundleName = string.Format("localdb_{1}", assetBundleName, languageCode);
                            break;
                        }
                    }
                }

                assetImporter.assetBundleName = assetBundleName;
                assetImporter.SaveAndReimport();
            }
            EditorApplication.SaveAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        return false;
    }

    WorksheetEntry FindWorksheetEntry(string worksheetName)
    {
        foreach (var spreadsheetEntry in m_SpreadsheetEntryList)
        {
            foreach (var worksheetEntry in spreadsheetEntry.Worksheets.Entries)
            {
                if (string.Equals(worksheetEntry.Title.Text, worksheetName))
                {
                    return worksheetEntry as WorksheetEntry;
                }
            }
        }

        return null;
    }

    string FindSpreadsheetName(string worksheetName)
    {
        foreach (var spreadsheetEntry in m_SpreadsheetEntryList)
        {
            foreach (var worksheetEntry in spreadsheetEntry.Worksheets.Entries)
            {
                if (string.Equals(worksheetEntry.Title.Text, worksheetName))
                {
                    return spreadsheetEntry.Title.Text;
                }
            }
        }

        return string.Empty;
    }

    void StartAuthenticate(string clientId, string clientSecret, string redirectUri, string scope)
    {
        m_OAuth2Parameters = new OAuth2Parameters();
        m_OAuth2Parameters.ClientId = clientId;
        m_OAuth2Parameters.ClientSecret = clientSecret;
        m_OAuth2Parameters.RedirectUri = redirectUri;
        m_OAuth2Parameters.Scope = scope;

        string authorizationUrl = OAuthUtil.CreateOAuth2AuthorizationUrl(m_OAuth2Parameters);
        Debug.Log(authorizationUrl);
        Debug.Log("Please visit the URL above to authorize your OAuth "
        + "request token.  Once that is complete, type in your access code to "
        + "continue...");
        System.Diagnostics.Process.Start(authorizationUrl);
    }

    bool FinishAuthenticate()
    {
        m_OAuth2Parameters = new OAuth2Parameters();
        m_OAuth2Parameters.ClientId = m_OAuth2Data.client_id;
        m_OAuth2Parameters.ClientSecret = m_OAuth2Data.client_secret;
        m_OAuth2Parameters.RedirectUri = m_OAuth2Data.redirect_uris[0];
        m_OAuth2Parameters.Scope = "https://spreadsheets.google.com/feeds https://docs.google.com/feeds";
        m_OAuth2Parameters.AccessCode = m_AccessCode;
        m_OAuth2Parameters.TokenType = "refresh";
        m_OAuth2Parameters.AccessType = "offline";
        OAuthUtil.GetAccessToken(m_OAuth2Parameters);

        if (!string.IsNullOrEmpty(m_OAuth2Parameters.AccessToken)
            && !string.IsNullOrEmpty(m_OAuth2Parameters.RefreshToken))
        {
            GoogleSheetsSettings.instance.m_AccessToken = m_OAuth2Parameters.AccessToken;
            GoogleSheetsSettings.instance.m_RefreshToken = m_OAuth2Parameters.RefreshToken;
            GoogleSheetsSettings.instance.m_GOAuth2RequestFactory = new GOAuth2RequestFactory(null, string.Empty, m_OAuth2Parameters);

            return isAuthenticated = true;
        }

        return isAuthenticated = false;
    }

    void RefreshWorksheetDataList()
    {
        //RefreshSpreadsheetEntryList();
        m_WorksheetDataList.Clear();
        if (m_SelectedSpreadsheetEntryList != null
            && m_SelectedSpreadsheetEntryList.Count > 0)
        {
            EditorUtility.DisplayProgressBar(string.Empty, string.Empty, 0);
            for (int i = 0; i < m_SelectedSpreadsheetEntryList.Count; i++)
            {
                SpreadsheetEntry spreadsheetEntry = m_SelectedSpreadsheetEntryList[i];
                Utility.DisplayProgressBar(spreadsheetEntry.Title.Text, string.Empty, i + 1, m_SelectedSpreadsheetEntryList.Count);
                if (spreadsheetEntry.Worksheets.Entries.Count > 0)
                {
                    for (int j = 0; j < spreadsheetEntry.Worksheets.Entries.Count; j++)
                    {
                        WorksheetEntry worksheetEntry = spreadsheetEntry.Worksheets.Entries[j] as WorksheetEntry;
                        Utility.DisplayProgressBar(spreadsheetEntry.Title.Text, worksheetEntry.Title.Text, j, spreadsheetEntry.Worksheets.Entries.Count);
                        m_WorksheetDataList.Add(new WorksheetData()
                            {
                                m_WorksheetEntry = worksheetEntry,
                            });
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    void RefreshSpreadsheetEntryList()
    {
        m_SpreadsheetEntryList.Clear();
        if (spreadsheetsService != null)
        {
            EditorUtility.DisplayProgressBar("SpreadsheetQuery", "0/1", 0);
            SpreadsheetFeed spreadSheetFeed = spreadsheetsService.Query(new SpreadsheetQuery());
            if (spreadSheetFeed != null)
            {
                for (int i = 0; i < spreadSheetFeed.Entries.Count; i++)
                {
                    SpreadsheetEntry spreadsheetEntry = spreadSheetFeed.Entries[i] as SpreadsheetEntry;
                    if (spreadsheetEntry.Title.Text.StartsWith("db_", StringComparison.OrdinalIgnoreCase) ||
                        spreadsheetEntry.Title.Text.StartsWith("dbstr_", StringComparison.OrdinalIgnoreCase))
                    {
                        m_SpreadsheetEntryList.Add(spreadsheetEntry);
                    }
                    EditorUtility.DisplayProgressBar(
                        spreadsheetEntry.Title.Text,
                        string.Format("{0}/{1}", (i + 1), spreadSheetFeed.Entries.Count),
                        ((float)(i + 1) / (float)spreadSheetFeed.Entries.Count));
                }

                if (m_SpreadsheetEntryList != null
                    && m_SpreadsheetEntryList.Count > 0)
                {
                    m_SpreadsheetEntryList.Sort((lhs, rhs) => lhs.Title.Text.CompareTo(rhs.Title.Text));
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    void RefreshScriptDataList()
    {
        m_ScriptDataList.Clear();
        string filePath = GoogleSheetsSettings.instance.m_ScriptFilePath;
        if (!Directory.Exists(filePath))
        {
            return;
        }
        EditorUtility.DisplayProgressBar(string.Empty, "0/1", 0);
        string[] files = Directory.GetFiles(filePath);
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = files[i];
            if (fileName.EndsWith(".meta"))
            {
                continue;
            }
            int startIndex = fileName.LastIndexOf('\\') + 1;
            if (startIndex == 0)
            {
                startIndex = fileName.LastIndexOf('/') + 1;
            }
            int length = fileName.Length - startIndex - 3; // 3 : .cs
            fileName = fileName.Substring(startIndex, length);
            if (string.Equals("Enum", fileName) || string.Equals("DBEnum_String", fileName) || fileName.Contains("ScriptableObject"))
            {
                continue;
            }
            m_ScriptDataList.Add(new ScriptData()
                {
                    m_Name = fileName,
                    m_Generate = false,
                });
        }
        EditorUtility.ClearProgressBar();
    }
}
