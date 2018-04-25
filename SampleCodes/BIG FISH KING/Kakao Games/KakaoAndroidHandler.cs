using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System;

public class KakaoAndroidHandler : MonoBehaviour, IKakaoNativeHandler
{
    private AndroidJavaObject mActivity;

    private AndroidJavaObject activity
    {
        get
        {
            if (mActivity == null)
            {
                AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                mActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
            }

            return mActivity;
        }
    }

    private Action<bool, int> mInitCallback;
    private Action<bool, int> mLogInCallback;
    private Action<bool, int> mLogOutCallback;
    private Action<bool, int, string> mUnlinkAppCallback;
    private Action<bool, int> mIsRegisteredMeCallback;
    private Action<bool, int> mShowMessageBlockDialog;

    #region IKakaoNativeHandler
    public void Init(Action<bool, int> callback)
    {
        Debug.Log("KakaoAndroidHandler.Init");
        mInitCallback = callback;
        activity.Call("Init");
    }

    public void LogIn(Action<bool, int> callback)
    {
        Debug.Log("KakaoAndroidHandler.LogIn");
        mLogInCallback = callback;
        activity.Call("Login");
    }

    public void LogOut(Action<bool, int> callback)
    {
        Debug.Log("KakaoAndroidHandler.LogOut");
        mLogOutCallback = callback;
        activity.Call("Logout");
    }

    public void UnlinkApp(Action<bool, int, string> callback)
    {
        Debug.Log("KakaoAndroidHandler.UnlinkApp");
        mUnlinkAppCallback = callback;
        activity.Call("Unlink");
    }

    public string GetAccessToken()
    {
        Debug.Log("KakaoAndroidHandler.GetAccessToken");
        return activity.Call<string>("GetAccessToken");
    }

    public string GetKAHeader()
    {
        Debug.Log("KakaoAndroidHandler.GetKAHeader");
        return activity.Call<string>("GetKA_header");
    }

    public void IsRegisteredMe(Action<bool, int> callback)
    {
        Debug.Log("KakaoAndroidHandler.IsRegisteredMe");
        mIsRegisteredMeCallback = callback;
        activity.Call("IsRegisterdMe");
    }

    // 게임 메시지 수신 거부 설정.
    public void ShowMessageBlockDialog(Action<bool, int> callback)
    {
        Debug.Log("KakaoAndroidHandler.ShowMessageBlockDialog");
        mShowMessageBlockDialog = callback;
        activity.Call("showMessageBlockDialog");
    }

    #endregion

    public void InitSuccess(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.InitSuccess");
        if (mInitCallback != null)
        {
            mInitCallback(true, KakaoNativeManager.SuccessCode);
        }

        mInitCallback = null;
    }

    public void InitFailed(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.InitFailed");
        if (mInitCallback != null)
        {
            KakaoCommon.Result result = JsonConvert.DeserializeObject<KakaoCommon.Result>(json_result);
            int errorCode = int.Parse(result.error_code);
            mInitCallback(false, errorCode);
        }

        mInitCallback = null;
    }

    public void LoginSuccess(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.LoginSuccess");
        if (mLogInCallback != null)
        {
            mLogInCallback(true, KakaoNativeManager.SuccessCode);
        }

        mLogInCallback = null;
    }

    public void LoginFailed(string json_result)
    {
        Debug.LogFormat("KakaoAndroidHandler.LoginFailed : {0}", json_result);
        if (mLogInCallback != null)
        {
            KakaoCommon.Result result = JsonConvert.DeserializeObject<KakaoCommon.Result>(json_result);

            if (result == null || result.error_code == null)
            {
                mLogInCallback(false, 2);
            }
            else
            {
                int errorCode;

                errorCode = int.Parse(result.error_code);
                mLogInCallback(false, errorCode);
            }
        }

        mLogInCallback = null;
    }

    public void LogoutSuccess(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.LogoutSuccess");
        if (mLogOutCallback != null)
        {
            mLogOutCallback(true, KakaoNativeManager.SuccessCode);
        }

        mLogOutCallback = null;
    }

    public void LogoutFailed(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.LogoutFailed");
        if (mLogOutCallback != null)
        {
            KakaoCommon.Result result = JsonConvert.DeserializeObject<KakaoCommon.Result>(json_result);
            int errorCode = int.Parse(result.error_code);
            mLogOutCallback(false, errorCode);
        }

        mLogOutCallback = null;
    }

    public void UnlinkSuccess(string json_result)
    {
        if (mUnlinkAppCallback != null)
        {
            mUnlinkAppCallback(true, 0, "");
        }

        mUnlinkAppCallback = null;
    }

    public void UnlinkFailed(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.UnlinkFailed");
        if (mUnlinkAppCallback != null)
        {
            KakaoCommon.Result result = JsonConvert.DeserializeObject<KakaoCommon.Result>(json_result);
            int errorCode = int.Parse(result.error_code);
            mUnlinkAppCallback(false, errorCode, result.error_message);
        }

        mUnlinkAppCallback = null;
    }

    public void IsRegisterdMeSuccess(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.IsRegisterdMeSuccess");
        Debug.LogFormat("json_result : {0}, mIsRegisteredMeCallback : {1}", json_result, mIsRegisteredMeCallback);
        if (mIsRegisteredMeCallback != null)
        {
            KakaoCommon.Result result = JsonConvert.DeserializeObject<KakaoCommon.Result>(json_result);
            bool success = ((KakaoCommon.Result.eErrorCode)result.code == KakaoCommon.Result.eErrorCode.RegisterdMe);
            Debug.Log(result);
            mIsRegisteredMeCallback(success, result.code);
        }

        mIsRegisteredMeCallback = null;
    }

    public void IsRegisterdMeFailed(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.IsRegisterdMeFailed");
        if (mIsRegisteredMeCallback != null)
        {
            KakaoCommon.Result result = JsonConvert.DeserializeObject<KakaoCommon.Result>(json_result);
            int errorCode = int.Parse(result.error_code);

            mIsRegisteredMeCallback(false, errorCode);
        }

        mIsRegisteredMeCallback = null;
    }

    public void ShowMessageBlockSuccess(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.ShowMessageBlockSuccess()" + json_result);
        if (mShowMessageBlockDialog != null)
        {
            mShowMessageBlockDialog(true, 0);
        }

        mShowMessageBlockDialog = null;
    }

    public void ShowMessageBlockFailed(string json_result)
    {
        Debug.Log("KakaoAndroidHandler.ShowMessageBlockFailed()" + json_result);
        if (mShowMessageBlockDialog != null)
        {
            mShowMessageBlockDialog(false, 0);
        }

        mShowMessageBlockDialog = null;
    }
}
