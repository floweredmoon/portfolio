using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;

public class WorksheetDataEditor : EditorWindow
{
    static WorksheetDataEditor m_Instance;

    public static WorksheetDataEditor instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = GetWindow<WorksheetDataEditor>();
                m_Instance.titleContent.text = "WorksheetData Editor";
            }

            return m_Instance;
        }
    }

    WorksheetData m_WorksheetData;
    Dictionary<uint, FieldData> m_FieldDataDictionary;
    Dictionary<string, EnumData> m_EnumDataDictionary;
    Vector2 m_DataListScrollPosition;

    void OnGUI()
    {
        if (m_WorksheetData != null)
        {
            m_DataListScrollPosition = EditorGUILayout.BeginScrollView(m_DataListScrollPosition, true, false, GUILayout.Width(position.width), GUILayout.MaxHeight(256));
            if (m_WorksheetData.isEnumWorksheet)
            {
                DrawEnumDataList();
            }
            else
            {
                DrawFieldDataList();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
            {
                // Shallow Copy
                m_WorksheetData.enumDataDictionary = m_EnumDataDictionary;
                m_WorksheetData.fieldDataDictionary = m_FieldDataDictionary;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Revert", GUILayout.ExpandWidth(false)))
            {

            }
            GUILayout.Space(10);
            if (GUILayout.Button("Close", GUILayout.ExpandWidth(false)))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }

    public void SetWorksheetData(WorksheetData worksheetData)
    {
        m_WorksheetData = worksheetData;

        if (worksheetData != null)
        {
            Utility.DisplayProgressBar(worksheetData.worksheetFullName, string.Empty, 0, 1);
            if (!worksheetData.initialized)
            {
                worksheetData.CellQuery();
            }

            // Shallow Copy
            m_EnumDataDictionary = new Dictionary<string, EnumData>(worksheetData.enumDataDictionary);
            m_FieldDataDictionary = new Dictionary<uint, FieldData>(worksheetData.fieldDataDictionary);
            EditorUtility.ClearProgressBar();
        }
    }

    void DrawEnumDataList()
    {
        if (m_EnumDataDictionary != null
            && m_EnumDataDictionary.Count > 0)
        {
            GUILayout.BeginHorizontal();
            foreach (EnumData enumData in m_EnumDataDictionary.Values)
            {
                if (enumData != null)
                {
                    GUILayout.BeginHorizontal();
                    enumData.generate = GUILayout.Toggle(enumData.generate, enumData.enumName, GUILayout.ExpandWidth(false));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    void DrawFieldDataList()
    {
        if (m_FieldDataDictionary != null
            && m_FieldDataDictionary.Count > 0)
        {
            GUILayout.BeginHorizontal();
            foreach (FieldData fieldData in m_FieldDataDictionary.Values)
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
