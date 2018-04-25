using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections;

public class SpreadsheetEditor : EditorWindow
{
    #region Variables
    static SpreadsheetEditor m_Instance;
    List<WorksheetData> m_WorksheetDataList = new List<WorksheetData>();
    StringBuilder m_SB = new StringBuilder();
    SpreadsheetEntry m_SpreadsheetEntry;
    GoogleSheetsImporter m_GoogleSheetsImporter;
    Vector2 scrollPosition;
    #endregion

    #region Properties
    public static SpreadsheetEditor instance
    {
        get
        {
            if (!m_Instance)
            {
                m_Instance = GetWindow<SpreadsheetEditor>();
                m_Instance.titleContent.text = "Spreadsheet Editor";
            }

            return m_Instance;
        }
    }
    #endregion

    void OnEnable()
    {
        m_WorksheetDataList.Clear();
        m_SB.Remove(0, m_SB.Length);
    }

    void OnGUI()
    {
        if (m_SpreadsheetEntry == null)
        {
            return;
        }

        DrawTitleContent(m_SpreadsheetEntry.Title.Text);
        GUILayout.Space(10);
        DrawWorksheetDataList(m_WorksheetDataList);
        GUILayout.Space(10);
        if (GUILayout.Button("Generate Script", GUILayout.Width(100), GUILayout.ExpandHeight(false)))
        {
            GenerateScript();
        }
        GUILayout.Space(10);
    }

    public void SetSpreadsheetEntry(GoogleSheetsImporter googleSheetImporter, SpreadsheetEntry spreadsheetEntry)
    {
        m_GoogleSheetsImporter = googleSheetImporter;
        m_SpreadsheetEntry = spreadsheetEntry;

        EditorUtility.DisplayProgressBar(string.Empty, string.Empty, 0);
        m_WorksheetDataList.Clear();
        if (m_SpreadsheetEntry != null)
        {
            float iValue = 0, iMaxValue = spreadsheetEntry.Worksheets.Entries.Count;
            for (int i = 0; i < spreadsheetEntry.Worksheets.Entries.Count; i++)
            {
                iValue = i;
                WorksheetEntry worksheetEntry = spreadsheetEntry.Worksheets.Entries[i] as WorksheetEntry;
                EditorUtility.DisplayProgressBar("CellQuery " + worksheetEntry.Title.Text,
                                                 string.Format("{0}/{1}", iValue, iMaxValue),
                                                 (iValue / iMaxValue));
                WorksheetData worksheetData = new WorksheetData()
                {
                    m_WorksheetEntry = worksheetEntry,
                };
                bool takeForm = true;
                SpreadsheetsService spreadsheetsService = m_GoogleSheetsImporter.spreadsheetsService;
                CellQuery cellQuery = new CellQuery(worksheetEntry.CellFeedLink);
                CellFeed cellFeed = spreadsheetsService.Query(cellQuery);

                float jValue = 0, jMaxValue = cellFeed.Entries.Count;
                for (int j = 0; j < cellFeed.Entries.Count; j++)
                {
                    jValue = j + 1;
                    CellEntry cellEntry = cellFeed.Entries[j] as CellEntry;
                    EditorUtility.DisplayProgressBar(worksheetData.worksheetFullName,
                                                     string.Format("{0} / {1}", jValue, jMaxValue),
                                                     (jValue / jMaxValue));
                    if (worksheetData.isEnumWorksheet)
                    {
                        if (Equals(cellEntry.Column, (uint)2))
                        {
                            string name = cellFeed[cellEntry.Row, 1].Value;
                            string enumerator = cellEntry.Value;
                            string comment = cellFeed[cellEntry.Row, 3].Value;

                            worksheetData.SetEnumData(name, enumerator, comment);
                        }
                    }
                    else
                    {
                        switch (cellEntry.Row)
                        {
                            case 1:
                                if (string.Equals("bool", cellEntry.Value, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals("int", cellEntry.Value, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals("float", cellEntry.Value, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals("string", cellEntry.Value, StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals("enum", cellEntry.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    worksheetData.SetFieldData(cellEntry.Column, string.Empty, cellEntry.Value);
                                }
                                else
                                {
                                    takeForm = false;
                                    break;
                                }
                                break;
                            case 2:
                                worksheetData.SetFieldData(cellEntry.Column, cellEntry.Value, string.Empty);
                                break;
                        }
                    }
                }

                if (takeForm)
                {
                    m_WorksheetDataList.Add(worksheetData);
                }
            }
        }
        EditorUtility.ClearProgressBar();
        Repaint();
    }

    void GenerateScript()
    {
        if (m_WorksheetDataList != null
            && m_WorksheetDataList.Count > 0)
        {
            string filePath = Utility.CompleteFilePath(GoogleSheetsSettings.instance.m_ScriptFilePath, true);
            List<string> fileNameList = new List<string>();
            foreach (WorksheetData worksheetData in m_WorksheetDataList)
            {
                if (worksheetData != null
                    && worksheetData.generate)
                {
                    string fileName = string.Empty;
                    if (worksheetData.isEnumWorksheet)
                    {
                        fileName = "Enum.cs";
                        Writer.WriteFile(filePath + fileName, Writer.WriteEnumCS(worksheetData));
                        fileNameList.Add(fileName);
                    }
                    else
                    {
                        // https://mseedgames.atlassian.net/browse/PUC-904
                        // (http://timtrott.co.uk/culture-codes/)
                        // KOR : Korea, Korean
                        // ENG : Unite States, English
                        // ZHO : China, Chinese
                        // JPN : Japan, Japanese
                        // FRA : France, French
                        // VIE : Vietnam, Vietnamese
                        // IND : Indonesia, Indonesian
                        // POR : Portugal, Portuguese
                        // DEU : Germany, German
                        // SPA : Spain, Spanish
                        // RUS : Russia, Russian
                        // TUR : Turkey, Turkish
                        // THA : Thailand, Thai

                        fileName = worksheetData.worksheetName + ".cs";
                        Writer.WriteFile(filePath + fileName, Writer.WriteTableObjectCS(worksheetData));
                        fileNameList.Add(fileName);

                        fileName = worksheetData.worksheetName + "ScriptableObject.cs";
                        Writer.WriteFile(filePath + fileName, Writer.WriteScriptableObjectCS(worksheetData));
                        fileNameList.Add(fileName);
                    }
                }
            }

            if (fileNameList != null
                && fileNameList.Count > 0)
            {
                m_SB.Remove(0, m_SB.Length);
                m_SB.AppendLine(filePath);
                m_SB.AppendLine();
                for (int i = 0; i < fileNameList.Count; i++)
                {
                    m_SB.AppendLine(fileNameList[i]);
                }

                EditorUtility.DisplayDialog(string.Empty, m_SB.ToString(), "OK");
                AssetDatabase.Refresh();
            }
        }
    }

    void DrawTitleContent(string titleContent)
    {
        if (this.titleContent.text != titleContent)
        {
            this.titleContent.text = titleContent;
        }
    }

    void DrawWorksheetDataList(List<WorksheetData> worksheetDataList)
    {
        if (worksheetDataList != null && worksheetDataList.Count > 0)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 100));
            for (int i = 0; i < worksheetDataList.Count; i++)
            {
                WorksheetData worksheetData = worksheetDataList[i];
                if (worksheetData != null)
                {
                    GUILayout.BeginHorizontal();
                    worksheetData.generate = GUILayout.Toggle(worksheetData.generate, worksheetData.worksheetFullName, GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                    if (worksheetData.isEnumWorksheet)
                    {
                        DrawWorksheetEnumDataList(worksheetData);
                    }
                    else
                    {
                        DrawWorksheetFieldDataList(worksheetData);
                    }
                    GUILayout.Space(10);
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }

    void DrawWorksheetEnumDataList(WorksheetData worksheetData)
    {
        if (worksheetData != null
            && worksheetData.isEnumWorksheet
            && worksheetData.enumDataDictionary != null
            && worksheetData.enumDataDictionary.Count > 0)
        {
            GUILayout.BeginHorizontal();
            foreach (EnumData enumData in worksheetData.enumDataDictionary.Values)
            {
                if (enumData != null)
                {
                    GUILayout.BeginVertical();
                    GUILayout.Toggle(enumData.generate, string.Empty, GUILayout.ExpandWidth(false));
                    GUILayout.Label(enumData.enumName, GUILayout.ExpandWidth(false));
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    void DrawWorksheetFieldDataList(WorksheetData worksheetData)
    {
        if (worksheetData != null
            && !worksheetData.isEnumWorksheet
            && worksheetData.fieldDataDictionary != null
            && worksheetData.fieldDataDictionary.Count > 0)
        {
            GUILayout.BeginHorizontal();
            foreach (FieldData fieldData in worksheetData.fieldDataDictionary.Values)
            {
                if (fieldData != null)
                {
                    GUILayout.BeginVertical();
                    fieldData.m_Generate = GUILayout.Toggle(fieldData.m_Generate, string.Empty, GUILayout.ExpandWidth(false));
                    GUILayout.Label(fieldData.m_FieldType.ToString(), GUILayout.ExpandWidth(false));
                    GUILayout.Label(fieldData.m_FieldName, GUILayout.ExpandWidth(false));
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}