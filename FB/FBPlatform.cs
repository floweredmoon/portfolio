using Facebook.Unity;
using Newtonsoft.Json;
using Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public sealed class FBPlatform : Singleton<FBPlatform>, ISocialPlatform
{

    public void ActivateAsync(Action<bool> callback = null)
    {
        StartCoroutine(ActivateCoroutine(callback));
    }

    public IEnumerator ActivateCoroutine(Action<bool> callback = null)
    {
        var keepWaiting = true;
        FB.Init(() =>
        {
            if (FB.IsInitialized)
            {
                //Social.Active = this;
                localUser = new FBLocalUser();

                if (Application.isMobilePlatform)
                    FB.ActivateApp(); // This only needs to be called for iOS or Android.
            }

            keepWaiting = false;
        });

        while (!keepWaiting)
            yield return null;

        callback.InvokeNullOk(FB.IsInitialized);
    }

    public void LogInWithReadPermissionsAsync(IEnumerable<string> permissions = null, Action<bool, AccessToken> callback = null)
    {
        StartCoroutine(LogInWithReadPermissionsCoroutine(permissions, callback));
    }

    public static IEnumerator LogInWithReadPermissionsCoroutine(IEnumerable<string> permissions = null, Action<bool, AccessToken> callback = null)
    {
        if (!FB.IsLoggedIn || !IsPermitted(permissions))
        {
            var keepWaiting = true;
            FB.LogInWithReadPermissions(permissions, (result) =>
            {
                keepWaiting = false;
            });

            while (keepWaiting)
                yield return null;
        }

        callback.InvokeNullOk(FB.IsLoggedIn && IsPermitted(permissions), AccessToken.CurrentAccessToken);
    }

    public void LogInWithPublishPermissionsAsync(IEnumerable<string> permissions = null, Action<bool, AccessToken> callback = null)
    {
        StartCoroutine(LogInWithPublishPermissionsCoroutine(permissions, callback));
    }

    public static IEnumerator LogInWithPublishPermissionsCoroutine(IEnumerable<string> permissions = null, Action<bool, AccessToken> callback = null)
    {
        if (!FB.IsLoggedIn || !IsPermitted(permissions))
        {
            var keepWaiting = true;
            FB.LogInWithPublishPermissions(permissions, (result) =>
            {
                keepWaiting = false;
            });

            while (keepWaiting)
                yield return null;
        }

        callback.InvokeNullOk(FB.IsLoggedIn && IsPermitted(permissions), AccessToken.CurrentAccessToken);
    }

    public static bool IsPermitted(IEnumerable<string> targetPermissions)
    {
        var isPermitted = true;
        if (targetPermissions != null)
            foreach (var targetPermission in targetPermissions)
                if (!(isPermitted &= IsPermitted(targetPermission)))
                    break;

        return isPermitted;
    }

    public static bool IsPermitted(string targetPermission)
    {
        var isPermitted = false;
        if (FB.IsLoggedIn)
            foreach (var permission in AccessToken.CurrentAccessToken.Permissions)
                if (isPermitted = string.Equals(permission, targetPermission))
                    break;

        return isPermitted;
    }

    public static void LogOut()
    {
        if (FB.IsLoggedIn)
            FB.LogOut();
    }

    public void RequestGraphApiAsync<TResponse>(string query, HttpMethod method, Action<bool, TResponse> callback = null, IDictionary<string, string> formData = null)
        where TResponse : class, IFBResponse
    {
        StartCoroutine(RequestGraphApiCoroutine(query, method, callback, formData));
    }

    public static IEnumerator RequestGraphApiCoroutine<TResponse>(string query, HttpMethod method, Action<bool, TResponse> callback = null, IDictionary<string, string> formData = null)
        where TResponse : class, IFBResponse
    {
        Debug.LogFormat("<color=#FF00DD>[{0}] {1}</color>", method, query);

        var keepWaiting = true;
        var successOrFailure = false;
        TResponse response = null;
        FB.API(
            query: query,
            method: method,
            callback: (result) =>
            {
                Debug.LogFormat("<color=#FF00DD>{0}</color>", result.ToStringNullOk());

                if (successOrFailure = result.SuccessOrFailure())
                    response = JsonConvert.DeserializeObject<TResponse>(result.RawResult);

                keepWaiting = false;
            },
            formData: formData);

        while (keepWaiting)
            yield return null;

        callback.InvokeNullOk(successOrFailure, response);
    }

    private IEnumerator LoadUsersCoroutine(string[] userIDs, Action<IUserProfile[]> callback)
    {
        List<FBUser> userProfiles = null;
        if (userIDs != null && userIDs.Length > 0)
        {
            userProfiles = new List<FBUser>(userIDs.Length);
            var responses = 0;
            for (var i = 0; i < userIDs.Length; i++)
            {
                RequestGraphApiAsync<FBUser>(
                query: FBUtility.QueryBuilder(userIDs[i], FBUser.Edge.picture, FBUser.Field.id, FBUser.Field.name),
                method: HttpMethod.GET,
                callback: (bool successOrFailure, FBUser userProfile) =>
                {
                    if (successOrFailure)
                        userProfiles.Add(userProfile);

                    responses++;
                });

                yield return null;
            }

            while (userIDs.Length != responses)
                yield return null;
        }

        callback.InvokeNullOk(userProfiles != null ? userProfiles.ToArray() : null);
    }

    #region ISocialPlatform

    public ILocalUser localUser
    {
        get;
        private set;
    }

    public void Authenticate(ILocalUser user, Action<bool> callback)
    {
        if (user != null)
            user.Authenticate(callback);
    }

    public void Authenticate(ILocalUser user, Action<bool, string> callback)
    {
        if (user != null)
            user.Authenticate(callback);
    }

    public IAchievement CreateAchievement()
    {
        return null;
    }

    public ILeaderboard CreateLeaderboard()
    {
        return null;
    }

    public bool GetLoading(ILeaderboard board)
    {
        return false;
    }

    public void LoadAchievementDescriptions(Action<IAchievementDescription[]> callback)
    {

    }

    public void LoadAchievements(Action<IAchievement[]> callback)
    {

    }

    public void LoadFriends(ILocalUser user, Action<bool> callback)
    {
        if (user != null)
            user.LoadFriends(callback);
    }

    public void LoadScores(string leaderboardID, Action<IScore[]> callback)
    {

    }

    public void LoadScores(ILeaderboard board, Action<bool> callback)
    {

    }

    public void LoadUsers(string[] userIDs, Action<IUserProfile[]> callback)
    {
        StartCoroutine(LoadUsersCoroutine(userIDs, callback));
    }

    public void ReportProgress(string achievementID, double progress, Action<bool> callback)
    {

    }

    public void ReportScore(long score, string board, Action<bool> callback)
    {

    }

    public void ShowAchievementsUI()
    {

    }

    public void ShowLeaderboardUI()
    {

    }

    #endregion

}
