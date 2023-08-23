using UnityEngine;
using UnityPost.Utils;

namespace UnityPost.WRE
{
    public class Authorization
    {
        public const string AUTH_SAVE_NAME = "authorization";

        public string access_token;
        public string token_type;
        public int expires_in;
        public string userName;

        public string issued;

        public string expires;

        public static Authorization FromJSON(string fromJson)
        {
            Authorization auth = JsonUtility.FromJson<Authorization>(fromJson);
            return auth;
        }

        public bool Save()
        {
            PlayerPrefs.SetString(AUTH_SAVE_NAME, JsonUtility.ToJson(this));
            return true;
        }

        public bool Load()
        {
            var auth = PlayerPrefs.GetString(AUTH_SAVE_NAME);
            if (auth.Length > 0)
            {
                Authorization newAuth = JsonUtility.FromJson<Authorization>(auth);
                newAuth.CopyPropertiesTo(this);
                return true;
            }

            return false;
        } 
    }
}