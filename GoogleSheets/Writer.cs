using System.IO;
using System.Text;

public static class Writer
{
    static StringBuilder m_SB = new StringBuilder();

    public static void WriteFile(string filePath, string value)
    {
        if (!string.IsNullOrEmpty(filePath)
            && !string.IsNullOrEmpty(value))
        {
            filePath = Utility.IntegrateFileSeparator(filePath);

            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.Write(value);
                streamWriter.Close();
            }
        }
    }

    public static string WriteScriptableObjectCS(WorksheetData worksheetData)
    {
        if (worksheetData != null
            && !worksheetData.isEnumWorksheet)
        {
            m_SB.Remove(0, m_SB.Length);
            m_SB.AppendLine("using System.Collections.Generic;");
            m_SB.AppendLine("using UnityEngine;");
            m_SB.AppendLine();
            m_SB.AppendFormat("public class {0}ScriptableObject : ScriptableObject", worksheetData.worksheetName);
            m_SB.AppendLine();
            m_SB.AppendLine("{");
            m_SB.AppendFormat("\tpublic List<{0}.Schema> m_SchemaList;", worksheetData.worksheetName);
            m_SB.AppendLine();
            m_SB.AppendLine("}");
            //m_SB.AppendLine();

            return m_SB.ToString();
        }

        return string.Empty;
    }

    public static string WriteTableObjectCS(WorksheetData worksheetData)
    {
        if (worksheetData != null
            && !worksheetData.isEnumWorksheet)
        {
            m_SB.Remove(0, m_SB.Length);
            m_SB.AppendLine("using System;");
            m_SB.AppendLine("using UnityEngine;");
            m_SB.AppendLine();
            m_SB.AppendFormat("public class {0} : TableBase<{0}, {0}.Schema>", worksheetData.worksheetName);
            m_SB.AppendLine();
            m_SB.AppendLine("{");
            m_SB.AppendLine("\t[Serializable]");
            m_SB.AppendLine("\tpublic class Schema");
            m_SB.AppendLine("\t{");

            if (worksheetData.fieldDataDictionary != null
                && worksheetData.fieldDataDictionary.Count > 0)
            {
                foreach (FieldData fieldData in worksheetData.fieldDataDictionary.Values)
                {
                    if (fieldData != null
                        && fieldData.m_Generate)
                    {
                        m_SB.AppendFormat("\t\tpublic {0} {1};", (fieldData.m_FieldType != FieldType.Enum) ? fieldData.m_FieldType.ToString().ToLowerInvariant() : fieldData.m_FieldName, fieldData.m_FieldName);
                        m_SB.AppendLine();
                    }
                }
            }

            m_SB.AppendLine("\t}");
            m_SB.AppendLine();
            m_SB.AppendLine("\tpublic static class Field");
            m_SB.AppendLine("\t{");

            if (worksheetData.fieldDataDictionary != null
                && worksheetData.fieldDataDictionary.Count > 0)
            {
                foreach (FieldData fieldData in worksheetData.fieldDataDictionary.Values)
                {
                    if (fieldData != null
                        && fieldData.m_Generate)
                    {
                        m_SB.AppendLine(string.Format("\t\tpublic static string {0} = \"{0}\";", fieldData.m_FieldName));
                    }
                }
            }

            m_SB.AppendLine("\t}");
            m_SB.AppendLine();

            #region
            m_SB.AppendLine("\tpublic override bool FromAssetBundle(AssetBundle assetBundle)");
            m_SB.AppendLine("\t{");
            m_SB.AppendLine("\t\tif (assetBundle != null)");
            m_SB.AppendLine("\t\t{");
            m_SB.AppendFormat("\t\t\tUnityEngine.Object asset = assetBundle.LoadAsset(\"{0}ScriptableObject\");", worksheetData.worksheetName); // GetType().ToString()
            m_SB.AppendLine();
            m_SB.AppendLine("\t\t\tif (asset != null)");
            m_SB.AppendLine("\t\t\t{");
            m_SB.AppendFormat("\t\t\t\t{0}ScriptableObject scriptableObject = asset as {0}ScriptableObject;", worksheetData.worksheetName);
            m_SB.AppendLine();
            m_SB.AppendLine("\t\t\t\tif (scriptableObject != null)");
            m_SB.AppendLine("\t\t\t\t{");
            m_SB.AppendLine("\t\t\t\t\treturn instance.SetSchemaList(scriptableObject.m_SchemaList);");
            m_SB.AppendLine("\t\t\t\t}");
            m_SB.AppendLine("\t\t\t}");
            m_SB.AppendLine("\t\t}");
            m_SB.AppendLine();
            m_SB.AppendLine("\t\treturn false;");
            m_SB.AppendLine("\t}");
            //m_SB.AppendLine();
            #endregion

            m_SB.AppendLine();

            #region
            m_SB.AppendLine("\tpublic override bool FromAssetBundle(AssetBundle assetBundle, string assetName)");
            m_SB.AppendLine("\t{");
            m_SB.AppendLine("\t\tif (assetBundle != null)");
            m_SB.AppendLine("\t\t{");
            m_SB.AppendLine("\t\t\tUnityEngine.Object asset = assetBundle.LoadAsset(assetName);");
            m_SB.AppendLine();
            m_SB.AppendLine("\t\t\tif (asset != null)");
            m_SB.AppendLine("\t\t\t{");
            m_SB.AppendFormat("\t\t\t\t{0}ScriptableObject scriptableObject = asset as {0}ScriptableObject;", worksheetData.worksheetName);
            m_SB.AppendLine();
            m_SB.AppendLine("\t\t\t\tif (scriptableObject != null)");
            m_SB.AppendLine("\t\t\t\t{");
            m_SB.AppendLine("\t\t\t\t\treturn instance.SetSchemaList(scriptableObject.m_SchemaList);");
            m_SB.AppendLine("\t\t\t\t}");
            m_SB.AppendLine("\t\t\t}");
            m_SB.AppendLine("\t\t}");
            m_SB.AppendLine();
            m_SB.AppendLine("\t\treturn false;");
            m_SB.AppendLine("\t}");
            #endregion

            m_SB.AppendLine("}");
            //m_SB.AppendLine();

            return m_SB.ToString();
        }

        return string.Empty;
    }

    public static string WriteEnumCS(WorksheetData worksheetData)
    {
        if (worksheetData != null
            && worksheetData.isEnumWorksheet)
        {
            m_SB.Remove(0, m_SB.Length);
            //m_SB.AppendLine("using System;");
            m_SB.AppendLine();

            if (worksheetData.enumDataDictionary != null
                && worksheetData.enumDataDictionary.Count > 0)
            {
                foreach (EnumData enumData in worksheetData.enumDataDictionary.Values)
                {
                    if (enumData != null
                        && enumData.generate)
                    {
                        m_SB.AppendFormat("public enum {0}", enumData.enumName);
                        m_SB.AppendLine();
                        m_SB.AppendLine("{");

                        int value = 0;
                        foreach (EnumData.ValueData valueData in enumData.valueDataDictionary.Values)
                        {
                            m_SB.AppendFormat("\t{0} = {1}, // {2}", valueData.valueName, ++value, valueData.valueAnnotation);
                            m_SB.AppendLine();
                        }

                        m_SB.AppendLine("}");
                        m_SB.AppendLine();
                    }
                }
            }

            return m_SB.ToString();
        }

        return string.Empty;
    }
}
