using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class WorksheetData
{
    #region Variables
    bool m_Generate;
    //static StringBuilder m_SB = new StringBuilder();
    public WorksheetEntry m_WorksheetEntry;
    Dictionary<uint, FieldData> m_FieldDataDictionary = new Dictionary<uint, FieldData>();
    Dictionary<string, EnumData> m_EnumDataDictionary = new Dictionary<string, EnumData>(StringComparer.Ordinal);
    string m_WorksheetName;
    #endregion

    #region Properties
    public bool generate
    {
        get
        {
            return m_Generate;
        }

        set
        {
            if (m_Generate != value)
            {
                m_Generate = value;
            }
        }
    }

    // Localization
    public string worksheetName
    {
        get
        {
            if (m_WorksheetEntry != null &&
                string.IsNullOrEmpty(m_WorksheetName))
            {
                m_WorksheetName = m_WorksheetEntry.Title.Text;

                foreach (LanguageCode languageCode in Enum.GetValues(typeof(LanguageCode)))
                {
                    if (m_WorksheetName.EndsWith(languageCode.ToString()/*, StringComparison.OrdinalIgnoreCase*/))
                    {
                        m_WorksheetName = m_WorksheetName.Replace("_" + languageCode.ToString(), string.Empty);
                        break;
                    }
                }
            }

            return m_WorksheetName;
        }
    }

    public string worksheetFullName
    {
        get
        {
            if (m_WorksheetEntry != null)
            {
                return m_WorksheetEntry.Title.Text;
            }

            return string.Empty;
        }
    }

    public bool isEnumWorksheet
    {
        get
        {
            if (m_WorksheetEntry != null)
            {
                return string.Equals("enum", m_WorksheetEntry.Title.Text, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }

    public Dictionary<uint, FieldData> fieldDataDictionary
    {
        get
        {
            return m_FieldDataDictionary;
        }

        set
        {
            if (m_FieldDataDictionary != value)
            {
                m_FieldDataDictionary = value;
            }
        }
    }

    public Dictionary<string, EnumData> enumDataDictionary
    {
        get
        {
            return m_EnumDataDictionary;
        }

        set
        {
            if (m_EnumDataDictionary != value)
            {
                m_EnumDataDictionary = value;
            }
        }
    }

    public bool initialized
    {
        get;
        set;
    }
    #endregion
    /*
    public override string ToString()
    {
        return worksheetName;
    }
    */
    public void SetEnumData(string enumName, string valueName, string valueAnnotation)
    {
        EnumData enumData = FindEnumData(enumName);
        if (enumData == null)
        {
            enumData = new EnumData()
            {
                enumName = enumName,
            };
            m_EnumDataDictionary.Add(enumName, enumData);
        }

        if (enumData != null)
        {
            enumData.AddValueData(valueName, valueAnnotation);
        }
    }

    public EnumData FindEnumData(string enumName)
    {
        if (m_EnumDataDictionary != null
            && m_EnumDataDictionary.Count > 0
            && m_EnumDataDictionary.ContainsKey(enumName))
        {
            return m_EnumDataDictionary[enumName];
        }

        return null;
    }

    public void SetFieldData(uint column, string fieldName, string fieldType)
    {
        FieldData fieldData;
        if (!m_FieldDataDictionary.TryGetValue(column, out fieldData))
        {
            m_FieldDataDictionary.Add(column, fieldData = new FieldData());
        }

        if (fieldData != null)
        {
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (fieldData.m_FieldName != fieldName)
                {
                    fieldData.m_FieldName = fieldName;
                }

                fieldData.m_Generate = (char.GetUnicodeCategory(fieldName[0]) != System.Globalization.UnicodeCategory.OtherLetter)
                                       && !fieldName.StartsWith("~"); // 임시
            }

            if (!string.IsNullOrEmpty(fieldType))
            {
                fieldData.m_FieldType = Utility.GetFieldType(fieldType);
            }
        }
    }

    public void CellQuery()
    {
        Utility.DisplayProgressBar(worksheetFullName, "SpreadsheetsService Query", 0, 1);
        CellQuery cellQuery = new CellQuery(m_WorksheetEntry.CellFeedLink);
        CellFeed cellFeed = GoogleSheetsImporter.instance.spreadsheetsService.Query(cellQuery);
        for (int i = 0; i < cellFeed.Entries.Count; i++)
        {
            CellEntry cellEntry = cellFeed.Entries[i] as CellEntry;
            Utility.DisplayProgressBar(worksheetFullName, string.Empty, i + 1, cellFeed.Entries.Count);
            if (isEnumWorksheet)
            {
                if (Equals(cellEntry.Column, (uint)2))
                {
                    string name = cellFeed[cellEntry.Row, 1].Value;
                    string enumerator = cellEntry.Value;
                    string comment = cellFeed[cellEntry.Row, 3].Value;

                    SetEnumData(name, enumerator, comment);
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
                            SetFieldData(cellEntry.Column, string.Empty, cellEntry.Value);
                        }
                        else
                        {
                            break;
                        }
                        break;
                    case 2:
                        SetFieldData(cellEntry.Column, cellEntry.Value, string.Empty);
                        break;
                }
            }
        }
        initialized = true;
        Utility.ClearProgressBar();
    }
}
