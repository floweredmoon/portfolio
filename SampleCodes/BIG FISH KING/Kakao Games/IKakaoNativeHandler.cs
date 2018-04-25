using System;

public interface IKakaoNativeHandler
{
    void Init(Action<bool, int> callback);

    void LogIn(Action<bool, int> callback);

    void LogOut(Action<bool, int> callback);

    void UnlinkApp(Action<bool, int, string> callback);

    string GetAccessToken();

    string GetKAHeader();

    void IsRegisteredMe(Action<bool, int> callback);

    void ShowMessageBlockDialog(Action<bool, int> callback);
}
