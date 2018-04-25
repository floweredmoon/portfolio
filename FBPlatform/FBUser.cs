using Facebook.Unity;
using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

// A user represents a person on Facebook. The /{user-id} node returns a single user.
// (https://developers.facebook.com/docs/graph-api/reference/user/)
[Serializable]
public class FBUser : IDisposable, IUserProfile, IFBNode
{

    // info about those "things", such as a person's birthday, or the name of a Page
    public enum Field
    {
        // The id of this person's user account. This ID is unique to each app and cannot be used across different apps.
        id,
        // The person's full name
        name,
    }

    // the connections between those "things", such as a Page's Photos, or a Photo's Comments
    public enum Edge
    {
        // A person's friends.
        friends,
        // The person's profile picture
        picture,
        // A list of friends that can be invited to install a Facebook Canvas app
        invitable_friends,
    }

    [Serializable]
    public sealed class edge_picture : IFBEdge
    {

        [Serializable]
        public class field_data
        {
            public bool is_silhouette;
            public string url;
        }

        public field_data data;
    }

    #region Fields

    public string name;
    public edge_picture picture;

    #endregion

    #region IUserProfile

    public string id
    {
        get;
        set;
    }

    public Texture2D image
    {
        get;
        set;
    }

    public bool isFriend
    {
        get
        {
            return !string.Equals(AccessToken.CurrentAccessToken.UserId, id);
        }
    }

    public UserState state
    {
        get;
        set;
    }

    public string userName
    {
        get
        {
            return name;
        }
    }

    #endregion

    #region IDisposable

    public virtual void Dispose()
    {
        name = string.Empty;
        picture = null;
        id = string.Empty;
        image = null;
        state = UserState.Offline;
    }

    #endregion

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
