using Facebook.Unity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

public sealed class FBLocalUser : FBUser, ILocalUser
{

    [Serializable]
    public sealed class edge_taggable_friends : IFBNodeCollection<FBUser>
    {

        [Serializable]
        public sealed class field_friends : FBPagination<field_friends, FBUser>
        {

            [Serializable]
            public class field_summary
            {
                public int total_count;
            }

            public field_summary summary;
        }

        public field_friends friends;

        #region IFBNodeCollection

        public List<FBUser> data
        {
            get
            {
                return friends != null ? friends.data : null;
            }
        }

        public bool hasNext
        {
            get
            {
                return friends != null && friends.paging != null ? friends.paging.hasNext : false;
            }
        }

        public int count
        {
            get
            {
                return friends != null ? friends.count : 0;
            }
        }

        public void RequestNextAsync(Action<bool> callback)
        {
            if (hasNext)
                friends.RequestNextAsync(callback);
            else
                callback.InvokeNullOk(false);
        }

        #endregion

    }

    [Serializable]
    public sealed class edge_invitable_friends : IFBNodeCollection<FBUser>
    {

        [Serializable]
        public sealed class field_invitable_friends : FBPagination<field_invitable_friends, FBUser>
        {

        }

        public field_invitable_friends invitable_friends;

        #region IFBNodeCollection

        public List<FBUser> data
        {
            get
            {
                return invitable_friends != null ? invitable_friends.data : null;
            }
        }

        public bool hasNext
        {
            get
            {
                return invitable_friends != null && invitable_friends.paging != null ? invitable_friends.paging.hasNext : false;
            }
        }

        public int count
        {
            get
            {
                return invitable_friends != null ? invitable_friends.count : 0;
            }
        }

        public void RequestNextAsync(Action<bool> callback)
        {
            if (hasNext)
                invitable_friends.RequestNextAsync(callback);
            else
                callback.InvokeNullOk(false);
        }

        #endregion

    }

    // me?fields=picture
    // {"picture":{"data":{"is_silhouette":true,"url":"https:\/\/scontent.xx.fbcdn.net\/v\/t1.0-1\/c15.0.50.50\/p50x50\/10354686_10150004552801856_220367501106153455_n.jpg?oh=b0e4037a0b04d52224a51836c84c322d&oe=59FEA22F"}},"id":"132659523974176"}
    [Serializable]
    public sealed new class edge_picture : IFBEdge
    {

        public FBUser.edge_picture picture;
        public string id;
    }

    public edge_taggable_friends taggable_friends;
    public edge_invitable_friends invitable_friends;

    #region ILocalUser

    public bool authenticated
    {
        get
        {
            return FB.IsLoggedIn;
        }
    }

    public IUserProfile[] friends
    {
        get
        {
            if (taggable_friends != null &&
                taggable_friends.friends != null &&
                taggable_friends.friends.data != null)
                return taggable_friends.friends.data.ToArray();
            else
                return null;
        }
    }

    public bool underage
    {
        get
        {
            return false;
        }
    }

    public void Authenticate(Action<bool> callback)
    {
        FBPlatform.instance.LogInWithReadPermissionsAsync(
            permissions: null,
            callback: (bool successOrFailure, AccessToken accessToken) =>
            {
                callback.InvokeNullOk(successOrFailure);
            });
    }

    public void Authenticate(Action<bool, string> callback)
    {
        FBPlatform.instance.LogInWithReadPermissionsAsync(
            permissions: null,
            callback: (bool successOrFailure, AccessToken accessToken) =>
            {
                callback.InvokeNullOk(successOrFailure, string.Empty);
            });
    }

    public void LoadFriends(Action<bool> callback)
    {
        if (taggable_friends == null)
        {
            var query = FBUtility.QueryBuilder("me", FBUtility.QueryBuilder(Edge.friends, null, Edge.picture, Field.id, Field.name));
            FBPlatform.instance.RequestGraphApiAsync<edge_taggable_friends>(
                query: query,
                method: HttpMethod.GET,
                callback: (bool successOrFailure, edge_taggable_friends taggable_friends) =>
                {
                    if (successOrFailure)
                        this.taggable_friends = taggable_friends;

                    callback.InvokeNullOk(successOrFailure);
                });
        }
        else
            taggable_friends.RequestNextAsync(callback);
    }

    #endregion

    #region IDisposable

    public override void Dispose()
    {
        base.Dispose();
        taggable_friends = null;
        invitable_friends = null;
    }

    #endregion

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
