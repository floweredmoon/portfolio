using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class Utility
{
    public static void DisplayProgressBar(string title, string info, float value, float maxValue)
    {
        EditorUtility.DisplayProgressBar(
            title,
            string.IsNullOrEmpty(info) ?
                string.Format("{0}/{1}", value, maxValue) :
                string.Format("{0} {1}/{2}", info, value, maxValue),
            (value / maxValue));
    }

    public static void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }

    public static FieldType GetFieldType(string fieldType)
    {
        if (!string.IsNullOrEmpty(fieldType))
        {
            fieldType = fieldType.ToLowerInvariant();

            switch (fieldType)
            {
                case "bool":
                    return FieldType.Bool;
                case "enum":
                    return FieldType.Enum;
                case "float":
                    return FieldType.Float;
                case "int":
                    return FieldType.Int;
                case "string":
                    return FieldType.String;
            }
        }

        Debug.LogError("");
        return FieldType.Unknown;
    }

    public static string CompleteFilePath(string filePath, bool createDirectory)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            filePath = IntegrateFileSeparator(filePath);
            if (!filePath.EndsWith("/"))
            {
                filePath = filePath + "/";
            }

            if (createDirectory && !Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        return filePath;
    }

    public static string IntegrateFileSeparator(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            filePath = filePath.Replace("\\", "/");
        }

        return filePath;
    }
}
