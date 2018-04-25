using System.Collections.Generic;
using UnityEngine;
using System;

public class EnumData
{
    public class ValueData
    {
        public string valueName
        {
            get;
            set;
        }

        public string valueAnnotation
        {
            get;
            set;
        }
    }

    #region Variables
    string m_EnumName;
    bool m_Generate = true;
    Dictionary<string, ValueData> m_ValueDataDictionary = new Dictionary<string, ValueData>(StringComparer.Ordinal);
    #endregion

    #region Properties
    public string enumName
    {
        get
        {
            return m_EnumName;
        }

        set
        {
            if (m_EnumName != value)
            {
                m_EnumName = value;
            }
        }
    }

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

    public Dictionary<string, ValueData> valueDataDictionary
    {
        get
        {
            return m_ValueDataDictionary;
        }
    }
    #endregion

    public void AddValueData(string valueName, string valueAnnotation)
    {
        if (!string.IsNullOrEmpty(valueName)
            && !m_ValueDataDictionary.ContainsKey(valueName))
        {
            m_ValueDataDictionary.Add(valueName, new ValueData()
                {
                    valueName = valueName,
                    valueAnnotation = valueAnnotation,
                });
        }
    }
}
