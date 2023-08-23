using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityPost.WRE
{
    public class WebRequestsEngine : MonoBehaviour
    {
        public static WebRequestsEngine Active
        {
            get
            {
                if (internalActive == null)
                {
                    internalActive = FindObjectOfType<WebRequestsEngine>();

                    if (internalActive == null)
                    {
                        internalActive = new GameObject("Web Requests Manager").AddComponent<WebRequestsEngine>();
                    }
                }

                //no point in preventing editor destruction if not in engine
#if !UNITY_EDITOR
                DontDestroyOnLoad(internalActive.gameObject);
#endif

                return internalActive;
            }
        }

        private static WebRequestsEngine internalActive;

        public string baseURL { private set; get; }
        public Authorization savedAuthentication { private set; get; }


        public void SetBaseURL(string url)
        {
            baseURL = url;
        }

        /// <summary>
        /// use this to log into API's that take in a username and password and return auth codes, the auth code will automatically be saved and used by the Web Requests Engine as long as it is valid
        /// </summary>
        public async Task Login(string username, string password)
        {
            WWWForm form = new WWWForm();
            form.AddField("grant_type", "password");
            form.AddField("Email", username);
            form.AddField("username", username);
            form.AddField("password", password);

            UnityWebRequest uwr = UnityWebRequest.Post(baseURL + "token", form);

            UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();

            //wait until we get a response
            while (operation.isDone == false)
            {
                await Task.Delay(10);
            }

            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("UWR Error: " + uwr.error);
            }
            else
            {
                Debug.Log("Successfully received new auth code, saving code");
                savedAuthentication = Authorization.FromJSON(uwr.downloadHandler.text);

                if (savedAuthentication.Save())
                {
                    Debug.Log("Successfully saved authorization to disk");
                }
            }
        }

        public async Task<string> GET(WebRequestItem requestItem)
        {
            string url = requestItem.PrependWithDefaultURL
                ? (baseURL + requestItem.url)
                : requestItem.url; //get the url to use
            UriBuilder builder = new UriBuilder(url);
            if (requestItem.mPort != -1)
                builder.Port = requestItem.mPort;

            string finalURL = builder.ToString();

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(finalURL);
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //add the parameters
            foreach (KeyValuePair<string, string> pair in requestItem.GetParameters())
            {
                httpWebRequest.Headers.Add(pair.Key, pair.Value);
            }

            //authentication handling
            if (requestItem.PreAuthenticate)
            {
                httpWebRequest.PreAuthenticate = true;
                httpWebRequest.Headers.Add("Authorization",
                    "Bearer " + (requestItem.customAuthentication.Length > 0
                        ? requestItem.customAuthentication
                        : savedAuthentication.access_token));
            }

            //only accept jsons
            //httpWebRequest.Accept = "application/json";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "GET";

            Debug.Log("Sending GET: " + httpWebRequest.Address);

            using (HttpWebResponse response = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var data = await reader.ReadToEndAsync();
                return data;
            }
        }

        public async Task<string> SET(WebRequestItem requestItem)
        {
            string url = requestItem.PrependWithDefaultURL
                ? (baseURL + requestItem.url)
                : requestItem.url; //get the url to use
            UriBuilder builder = new UriBuilder(url);

            if (requestItem.mPort != -1)
                builder.Port = requestItem.mPort;

            string finalURL = builder.ToString();


            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(finalURL);
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //add the parameters
            foreach (KeyValuePair<string, string> pair in requestItem.GetParameters())
            {
                httpWebRequest.Headers.Add(pair.Key, pair.Value);
            }

            httpWebRequest.Headers.Add("Access-Control-Allow-Origin", "*");

            //authentication handling
            if (requestItem.PreAuthenticate)
            {
                httpWebRequest.PreAuthenticate = true;
                httpWebRequest.Headers.Add("Authorization",
                    "Bearer " + (requestItem.customAuthentication.Length > 0
                        ? requestItem.customAuthentication
                        : savedAuthentication.access_token));
            }

            //only accept jsons
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";

            Console.Write("Sending SET: " + httpWebRequest.Address);


            using (HttpWebResponse response = (HttpWebResponse)await httpWebRequest.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var data = await reader.ReadToEndAsync();
                return data;
            }
        }


        public class WebRequestItem
        {
            private Dictionary<string, string> parameters = new Dictionary<string, string>();
            public string url { private set; get; }
            public bool PrependWithDefaultURL = true;

            /// <summary>
            /// should this request attempt to pre-authenticate with the server,
            /// we shall use the default authentication key saved in the player settings
            /// </summary>
            public bool PreAuthenticate;

            public string customAuthentication { private set; get; }

            public int mPort { private set; get; } = -1;


            /// <summary>
            /// the additional url of this request
            /// </summary>
            /// <param name="urlAddition"></param>
            public WebRequestItem(string urlAddition)
            {
                url = urlAddition;
                customAuthentication = "";
            }

            public void AddParameters(Dictionary<string, string> n_ps)
            {
                foreach (KeyValuePair<string, string> pair in n_ps)
                {
                    AddParameter(pair.Key, pair.Value);
                }
            }

            public void AddParameter(string key, string value)
            {
                parameters.Add(key, value);
            }

            public void SetCustomAuthentication(string auth)
            {
                customAuthentication = auth;
            }

            public void SetPort(int port)
            {
                mPort = port;
            }

            public Dictionary<string, string> GetParameters()
            {
                return new Dictionary<string, string>(parameters);
            }
        }
    }
}