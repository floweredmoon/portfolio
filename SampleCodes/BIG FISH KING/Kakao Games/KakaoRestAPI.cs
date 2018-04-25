using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

public class RestAPICall<ResultType>
{
    public enum Method
    {
        Post,
        Get,
    }

    // HTTP Status Code
    // 200인 경우는 해당 요청에 맞는 response body가 내려가고, 그외의 경우는 "code"와 "msg"로 이뤄진 json response body가 내려갑니다.
    // Code    상태
    // 200	API 호출 성공.Response Body는 각 API 별로 다를 수 있음
    // 400	잘못된 요청. 주로 API에 필요한 필수 파라미터와 관련
    // 401	인증 오류. Unauthorized, Invalid Token 등 주로 사용자 토큰과 관련
    // 403	권한/퍼미션 등의 오류
    // 500	내부 서버 에러
    // 503	점검중
    const int Success = 200;

    string resource;
    Method method;

    public Dictionary<string, string> header = new Dictionary<string, string>();

    public RestAPICall(string resource, Method method)
    {
        this.resource = resource;
        this.method = method;
    }

    public IEnumerator Post(WWWForm form, Action<bool, ResultType> resultCallback, bool errorMsgPopup = true)
    {
        HTTP.Request req = null;
        if (form != null) req = new HTTP.Request(resource, form);
        else req = new HTTP.Request("POST", resource);
        foreach (KeyValuePair<string, string> headerPair in header)
        {
            req.headers.Set(headerPair.Key, headerPair.Value);
        }

        Debug.Log(resource);
        Debug.Log(req.headers.ToString());

        yield return req.Send();

        bool success = ResponseHandler(errorMsgPopup, req.response);
        if (resultCallback != null)
        {
            ResultType result = default(ResultType);
            if (success) result = JsonConvert.DeserializeObject<ResultType>(req.response.Text);

            resultCallback(success, result);
        }
    }

    public IEnumerator Get(HttpGetParameter get, System.Action<bool, ResultType> resultCallback, bool errorMsgPopup = true)
    {
        string url = "";
        if (get == null || get.Count == 0)
            url = resource;
        else
            url = string.Format("{0}?{1}", resource, get.GetParameter());

        HTTP.Request req = new HTTP.Request("GET", url);

        foreach (KeyValuePair<string, string> headerPair in header)
        {
            req.headers.Set(headerPair.Key, headerPair.Value);
        }

        Debug.Log(url);
        Debug.Log(req.headers.ToString());

        yield return req.Send();

        bool success = ResponseHandler(errorMsgPopup, req.response);

        if (resultCallback != null)
        {
            ResultType result = default(ResultType);
            if (success)
            {
                result = JsonConvert.DeserializeObject<ResultType>(req.response.Text);
            }

            resultCallback(success, result);
        }
    }

    private class ErrorResult
    {
        public int code;
        public string msg;
    }

    private bool ResponseHandler(bool errorMsgPopup, HTTP.Response response)
    {
		Debug.LogFormat("errorMsgPopup : {0}, response.status : {1}", errorMsgPopup, response.status);
        if (response.status != Success)
        {
            ErrorResult errorResult = JsonConvert.DeserializeObject<ErrorResult>(response.Text);
            Debug.LogFormat("code : {0}, msg : {1}", errorResult.code, errorResult.msg);

            if (errorMsgPopup && errorResult.code < 0)
            {
                UI_PopupManager.sOpenKakaoMessagePopup(errorResult.code, errorResult.msg);
            }

            return false;
        }

        return true;
    }
}

public class ParamKVPair
{
    public string key;
    public string value;
}

public class HttpGetParameter : List<ParamKVPair>
{
    public void Add(string key, string value)
    {
        var pair = new ParamKVPair();
        pair.key = key;
        pair.value = value;

        Add(pair);
    }

    public string GetParameter()
    {
        string param = "";

        for (int i = 0; i < Count;)
        {
            param += string.Format("{0}={1}", this[i].key, this[i].value);
            i++;

            if (i < Count)
            {
                param += "&";
            }
        }

        return param;
    }
}

public class KakaoRestAPI : kSingletonPersistent<KakaoRestAPI>
{
    public enum friend_filter
    {
        none,
        registered,
        invitable,
    }

    public enum receiver_id_type
    {
        user_id,
        uuid,
        chat_id,
    }

    public enum template_id
    {
        _232 = 232,
        _233 = 233,
    }

    const string HostAddress = "https://game-kapi.kakao.com";

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static RestAPICall<ResultType> CreateCall<ResultType>(string url, RestAPICall<ResultType>.Method method)
    {
        string resource = HostAddress + url;
        var call = new RestAPICall<ResultType>(resource, method);
        var header = new Dictionary<string, string>();

        call.header["Authorization"] = "Bearer " + KakaoNativeManager.GetAccessToken();
        call.header["KA"] = KakaoNativeManager.GetKAHeader();

        return call;
    }

    #region RequestMe
    public class MyInfo
    {
        public class Properties
        {
            public string msg_blocked { get; set; }

            public bool IsBlocked()
            {
                if (msg_blocked == null)
                {
                    return false;
                }
                else
                {
                    bool isBlocked = false;
                    bool.TryParse(msg_blocked, out isBlocked);

                    return isBlocked;
                }
            }
        }

        public long id { get; set; }
        public string uuid { get; set; }
        public long service_user_id { get; set; }
        public int remaining_invite_count { get; set; }
        public int remaining_group_msg_count { get; set; }

        public Properties properties = new Properties();
    }

    public static void cRequestMe(System.Action<bool, MyInfo> callBack)
    {
        if (Instance != null)
        {
            Debug.Log("KakaoRestAPI.cRequestMe");
            var call = CreateCall<MyInfo>("/v1/user/me", RestAPICall<MyInfo>.Method.Get);

            Instance.StartCoroutine(call.Get(null, callBack, false));
        }
    }
    #endregion

    #region Profile
    // Profile
    public class ProfileData
    {
        public string nickName;
        public string profileImageURL;      // IOS를 위해 https 로 받아야함
        public string thumbnailURL;         // IOS를 위해 https 로 받아야함
        public string countryISO;
    };

    public static void cRequestProfile(System.Action<bool, ProfileData> callBack)
    {
        if (Instance != null)
        {
            Debug.Log("KakaoRestAPI.cRequestProfile");
            var call = CreateCall<ProfileData>("/v1/api/talk/profile", RestAPICall<ProfileData>.Method.Get);

            Instance.StartCoroutine(call.Get(null, callBack, false));
        }
    }
    #endregion

    #region FriendList
    // FriendList
    public class FriendRelation
    {
        public string talk;
        public string story;
    }

    public class FriendInfo
    {
        public long? id;
        public string uuid;
        public long service_user_id;
        public bool app_registered;
        public string profile_nickname;
        public string profile_thumbnail_image;
        public string talk_os;
        public bool allowed_msg;
        public FriendRelation relation;
    };

    public class FriendList
    {
        public List<FriendInfo> elements;
        public int total_count;
        string before_url;
        string after_url;
        string result_id;
    }

    public static void cRequestFriendList(friend_filter friendFilter, int offset, int limit, System.Action<bool, FriendList> callBack)
    {
        if (Instance != null)
        {
            Debug.Log("KakaoRestAPI.cRequestFriendList");
            var call = CreateCall<FriendList>("/v1/friends", RestAPICall<FriendList>.Method.Get);
            System.Action<bool, FriendList> callback = (success, result) =>
            {
                switch (friendFilter)
                {
                    case friend_filter.invitable:
                        KakaoFriends.Instance.SetInvitableFriendList(result);
                        break;
                    case friend_filter.none:
                        break;
                    case friend_filter.registered:
                        KakaoFriends.Instance.SetRegisteredFriendList(result);
                        break;
                }

                if (callBack != null)
                    callBack(success, result);
            };

            HttpGetParameter get = new HttpGetParameter();
            get.Add("friend_type", "talk");
            get.Add("friend_filter", friendFilter.ToString());
            get.Add("offset", offset.ToString());
            get.Add("limit", limit.ToString());

            Instance.StartCoroutine(call.Get(get, callback));
        }
    }
    #endregion

    #region Unlink
    public class unlink_response
    {
        public long id;
    }
    // 탈퇴.
    public static void cRequestUnlink(Action<bool, unlink_response> callBack)
    {
        //[Response]
        //앱 연결 해제 요청이 성공하면 응답 바디에 JSON 객체로 아래 값을 포함합니다.
        //Name    Type            Description
        //id      signed int64    앱 연결 해제된 사용자 ID
        if (Instance != null)
        {
            Debug.Log("KakaoRestAPI.cRequestUnlink");
            var call = CreateCall<unlink_response>("/v1/user/unlink", RestAPICall<unlink_response>.Method.Post);

            Instance.StartCoroutine(call.Post(null, callBack));
        }
    }
    #endregion

    #region SendMessage
    public class send_result_code
    {
        public int result_code;
    }

    // 카카오톡 메시지 전송
    public static void cRequestSendMessage(string receiver_id, receiver_id_type receiverIdType, template_id templateId, string args, Action<bool, send_result_code> callBack)
    {
        Debug.LogFormat("receiver_id : {0}, receivier_id_type : {1}, template_id : {2}, args : {3}", receiver_id, receiverIdType, templateId, args);
        //[Response]
        //응답 바디는 결과 코드를 포함합니다.
        //Name        Type    Description
        //result_code Integer 전송 성공 : 0
        if (Instance != null)
        {
            Debug.Log("KakaoRestAPI.cRequestSendMessage");
            var call = CreateCall<send_result_code>("/v1/api/talk/message/send", RestAPICall<send_result_code>.Method.Post);

            WWWForm form = new WWWForm();
            form.AddField("receiver_id", receiver_id);
            form.AddField("receiver_id_type", receiverIdType.ToString());
            form.AddField("template_id", ((int)templateId).ToString());
            if (!string.IsNullOrEmpty(args))
                form.AddField("args", args);

            Instance.StartCoroutine(call.Post(form, callBack));
        }
    }
    #endregion

    #region IsStoryUser
    public class isstoryuser_result
    {
        public bool isStoryUser;
    }

    // 사용자가 카카오스토리 가입자인지 확인합니다.
    public static void RequestIsStoryUser(Action<bool, bool> callBack)
    {
        // [Response] 요청이 성공하면 응답 바디에 JSON 객체로 아래 값을 포함합니다.
        // Name         Type    Description
        // isStoryUser  Boolean 카카오스토리 사용자인지의 여부. true일 경우 카카오스토리 가입자.
        if (Instance != null)
        {
            Debug.Log("KakaoRestAPI.RequestIsStoryUser");
            var call = CreateCall<isstoryuser_result>("/v1/api/story/isstoryuser", RestAPICall<isstoryuser_result>.Method.Get);
            Action<bool, isstoryuser_result> callback = (success, result) =>
            {
                if (callBack != null)
                {
                    callBack(success, result.isStoryUser);
                }
            };
            Instance.StartCoroutine(call.Get(null, callback));
        }
    }
    #endregion
}
