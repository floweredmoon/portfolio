using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class KakaoNativeManager : kSingletonPersistent<KakaoNativeManager>
{
    public readonly static string SDKVersion = "2.0.0";
    public const int SuccessCode = 200;

    private static IKakaoNativeHandler mKakaoNativeHandler = null;
    private bool mInitialized = false;

    public KakaoRestAPI.MyInfo myInfo = new KakaoRestAPI.MyInfo();
    public KakaoRestAPI.ProfileData profileData = new KakaoRestAPI.ProfileData();

    private void Awake()
    {
        // 안드로이드, iOS 각 네이티브 코드에서 Kakao 게임 오브젝트 이름으로 SendMessage를 호출함.
        gameObject.name = "Kakao";
        Debug.LogFormat("KakaoNativeManager.gameObject.name is {0}", gameObject.name);
    }

    // Use this for initialization

    // Update is called once per frame

    #region IKakaoNativeHandler
    public void Init(Action<bool, int> callback)
    {
        if (mInitialized)
        {
            Debug.LogWarning("KakaoNativeManager is already initialized.");
            //return;
        }

#if UNITY_ANDROID
        mKakaoNativeHandler = gameObject.AddComponent<KakaoAndroidHandler>();
#elif UNITY_IOS
        mKakaoNativeHandler = gameObject.AddComponent<KakaoIOSHandler>();
#else
        Debug.LogErrorFormat("KakaoManager not supported {0} platform.", Application.platform);
#endif

        if (mKakaoNativeHandler != null)
        {
            mKakaoNativeHandler.Init(callback);
            Debug.LogFormat("KakaoNativeManager is initialized for {0}.", Application.platform);
            mInitialized = true;
        }
        else
        {
            Debug.LogError("Failed to create instance of IKakaoNativeHandler.");

            if (callback != null)
            {
                callback(false, 0);
            }
        }
    }

    public void LogIn(Action<bool, int> callback)
    {
        if (mKakaoNativeHandler != null)
        {
            mKakaoNativeHandler.LogIn(callback);
        }
        else
        {
            Debug.LogError("Failed to LogIn. (KakaoNativeManager is not initialized.)");
        }
    }

    public void LogOut(Action<bool, int> callback)
    {
        if (mKakaoNativeHandler != null)
        {
            mKakaoNativeHandler.LogOut(callback);
        }
        else
        {
            Debug.LogError("Failed to LogOut. (KakaoNativeManager is not initialized.)");
        }
    }

    public void UnlinkApp(Action<bool, int, string> callback)
    {
        if (mKakaoNativeHandler != null)
        {
            mKakaoNativeHandler.UnlinkApp(callback);
        }
        else
        {
            Debug.LogError("Failed to UnlinkApp. (KakaoNativeManager is not initialized.)");
        }
    }

    public static string GetAccessToken()
    {
        if (mKakaoNativeHandler != null)
        {
            return mKakaoNativeHandler.GetAccessToken();
        }
        else
        {
            Debug.LogError("Failed to GetAccessToken. (KakaoNativeManager is not initialized.)");
            return string.Empty;
        }
    }

    public static string GetKAHeader()
    {
        if (mKakaoNativeHandler != null)
        {
            return mKakaoNativeHandler.GetKAHeader();
        }
        else
        {
            Debug.LogError("Failed to GetKAHeader. (KakaoNativeManager is not initialized.)");
            return string.Empty;
        }
    }

    public void IsRegisterdMe(Action<bool, int> callback)
    {
        Debug.Log("KakaoNativeManager.IsRegisteredMe");
        if (mKakaoNativeHandler != null)
        {
            mKakaoNativeHandler.IsRegisteredMe(callback);
        }
        else
        {
            Debug.LogError("Failed to IsRegisteredMe. (KakaoNativeManager is not initialized.)");
        }
    }

    public void ShowMessageBlockDialog(Action<bool, int> callback)
    {
        Debug.Log("KakaoNativeManager.ShowMessageBlockDialog");
        if (mKakaoNativeHandler != null)
        {
            mKakaoNativeHandler.ShowMessageBlockDialog(callback);
        }
        else
        {
            Debug.LogError("Failed to ShowMessageBlockDialog. (KakaoNativeManager is not initialized.)");
        }
    }
    #endregion
}
