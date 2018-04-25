using Google.GData.Client;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct OAuth2Data
{
    public string client_id;
    public string auth_uri;
    public string token_uri;
    public string auth_provider_x509_cert_url;
    public string client_secret;
    public List<string> redirect_uris;
}

public class GoogleSheetsSettings : ScriptableObject
{
    static GoogleSheetsSettings m_Instance;

    public static GoogleSheetsSettings instance
    {
        get
        {
            if (!m_Instance)
            {
                Instantiate();
            }

            return m_Instance;
        }
    }

    public OAuth2Data m_OAuth2Data;
    public string m_AccessCode;
    public string m_AccessToken;
    public string m_RefreshToken;
    public GOAuth2RequestFactory m_GOAuth2RequestFactory;
    public string m_ClientSecretFilePath;
    public string m_ScriptFilePath;
    public string m_ScriptableObjectFilePath;
    public string m_XmlFilePath;
    public string m_AssetBundleFilePath;

    public static void Instantiate()
    {
        if (!m_Instance)
        {
            m_Instance = AssetDatabase.LoadAssetAtPath(
                "Assets/GoogleSheetsSettings.asset",
                typeof(GoogleSheetsSettings)) as GoogleSheetsSettings;
        }

        if (!m_Instance)
        {
            m_Instance = CreateInstance<GoogleSheetsSettings>();
            // GoogleSheetsImporter.cs, 94 lines.
            m_Instance.m_OAuth2Data = new OAuth2Data()
            {
                client_id = "755184654858-4p5da1606aa6nlhlfe7d188iiqa6099p.apps.googleusercontent.com",
                auth_uri = "https://accounts.google.com/o/oauth2/auth",
                token_uri = "https://accounts.google.com/o/oauth2/token",
                auth_provider_x509_cert_url = "https://www.googleapis.com/oauth2/v1/certs",
                client_secret = "8vNsGRSPgdgf6Sg5qQIbk86x",
                redirect_uris = new List<string>() { "urn:ietf:wg:oauth:2.0:oob", "http://localhost" },
            };
            if (string.IsNullOrEmpty(m_Instance.m_ScriptFilePath))
            {
                m_Instance.m_ScriptFilePath = "Assets/Scripts/";
            }
            if (string.IsNullOrEmpty(m_Instance.m_ScriptableObjectFilePath))
            {
                m_Instance.m_ScriptableObjectFilePath = "Assets/Resources/ScriptableObject/";
            }
            if (string.IsNullOrEmpty(m_Instance.m_XmlFilePath))
            {
                m_Instance.m_XmlFilePath = "Assets/Resources/Xml/";
            }
            if (string.IsNullOrEmpty(m_Instance.m_AssetBundleFilePath))
            {
                m_Instance.m_AssetBundleFilePath = "Assets/Resources/AssetBundle/";
            }

            AssetDatabase.CreateAsset(m_Instance, "Assets/GoogleSheetsSettings.asset");
            EditorUtility.SetDirty(m_Instance);
            EditorApplication.SaveAssets();
            AssetDatabase.SaveAssets();
        }

        Selection.activeObject = m_Instance;
    }

    public void Save()
    {
        if (instance != null)
        {
            EditorUtility.SetDirty(GoogleSheetsSettings.instance);
            EditorApplication.SaveAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
