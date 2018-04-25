using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class KakaoIOSHandler : MonoBehaviour, IKakaoNativeHandler
{
    private Action<bool, int> mLogInCallback;
    private Action<bool, int> mLogOutCallback;
    private Action<bool, int, string> mUnlinkAppCallback;
    private Action<bool, int> mIsRegisteredMeCallback;
    private Action<bool, int> mMessageBlockCallback;

    // Use this for initialization

    // Update is called once per frame

    #region IKakaoNativeHandler
    public void Init(Action<bool, int> callback)
    {
        // iOS는 초기화 처리가 없음.
        if (callback != null)
        {
            callback(true, KakaoNativeManager.SuccessCode);
        }
    }

    public void LogIn(Action<bool, int> callback)
    {
        mLogInCallback = callback;
        _LOGIN();
    }

    public void LogOut(Action<bool, int> callback)
    {
        mLogOutCallback = callback;
        _LOGOUT();
    }

    public void UnlinkApp(Action<bool, int, string> callback)
    {
        mUnlinkAppCallback = callback;
        _UNLINKAPP();
    }

    public string GetAccessToken()
    {
        return Marshal.PtrToStringAnsi(getAccessToken());
    }

    public string GetKAHeader()
    {
        return Marshal.PtrToStringAnsi(getAccessToken());
    }

    public void IsRegisteredMe(Action<bool, int> callback)
    {
        mIsRegisteredMeCallback = callback;
        _CHECKANDIMPLICITOPEN();
    }

    public static string GetIDFA()
    {
        return Marshal.PtrToStringAnsi(getIDFA());
    }

    public void ShowMessageBlockDialog(Action<bool, int> callback)
    {
        mMessageBlockCallback = callback;
        _SHOWMESSAGEBLOCKDIALOG();
    }
    #endregion

    private void _LOGIN()
    {
        logIn(gameObject.name, "_HANDLE_LOGIN");
    }

    private void _LOGOUT()
    {
        logOut(gameObject.name, "_HANDLE_LOGOUT");
    }

    private void _UNLINKAPP()
    {
        unlinkApp(gameObject.name, "_HANDLE_UNLINKAPP");
    }

    private void _CHECKANDIMPLICITOPEN()
    {
        checkAndImplicitOpen(gameObject.name, "_HANDLE_CHECKANDIMPLICITOPEN");
    }

    private void _SHOWMESSAGEBLOCKDIALOG()
    {
        showMessageBlockDialog(gameObject.name, "_HANDLE_SHOWMESSAGEBLOCKDIALOG");
    }

    public void _HANDLE_LOGIN(string msg)
    {
        _HANDLE_MESSAGE(msg, mLogInCallback);
        mLogInCallback = null;
    }

    public void _HANDLE_LOGOUT(string msg)
    {
        _HANDLE_MESSAGE(msg, mLogOutCallback);
        mLogOutCallback = null;
    }

    public void _HANDLE_UNLINKAPP(string msg)
    {
        _HANDLE_MESSAGE(msg, mUnlinkAppCallback);
        mUnlinkAppCallback = null;
    }

    public void _HANDLE_CHECKANDIMPLICITOPEN(string msg)
    {
        _HANDLE_MESSAGE(msg, mIsRegisteredMeCallback);
        mIsRegisteredMeCallback = null;
    }

    public void _HANDLE_SHOWMESSAGEBLOCKDIALOG(string msg)
    {
        _HANDLE_MESSAGE(msg, mMessageBlockCallback);
        mMessageBlockCallback = null;
    }

    private void _HANDLE_MESSAGE(string msg, Action<bool, int> callback)
    {
        if (callback != null)
        {
            bool success = string.IsNullOrEmpty(msg);
            int errorCode = KakaoNativeManager.SuccessCode;
            if (!success)
            {
                errorCode = int.Parse(msg);
            }

            callback(success, errorCode);
        }
    }

    private void _HANDLE_MESSAGE(string msg, Action<bool, int, string> callback)
    {
        if (callback != null)
        {
            bool success = string.IsNullOrEmpty(msg);
            int errorCode = KakaoNativeManager.SuccessCode;
            if (!success)
            {
                errorCode = int.Parse(msg);
            }

            callback(success, errorCode, "");
        }
    }

    [DllImport("__Internal")]
    private static extern void logIn(string obj, string method);

    [DllImport("__Internal")]
    private static extern void logOut(string obj, string method);

    [DllImport("__Internal")]
    private static extern void unlinkApp(string obj, string method);

    [DllImport("__Internal")]
    private static extern IntPtr getAccessToken();

    [DllImport("__Internal")]
    private static extern IntPtr getKAHeader();

    [DllImport("__Internal")]
    private static extern void checkAndImplicitOpen(string obj, string method);

    [DllImport("__Internal")]
    public static extern void copyToClipboard(string message);

    [DllImport("__Internal")]
    private static extern IntPtr getIDFA();

    [DllImport("__Internal")]
    private static extern void showMessageBlockDialog(string obj, string method);

    [DllImport("__Internal")]
    public static extern void invalidation();

    [DllImport("__Internal")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool isOpen();
}
