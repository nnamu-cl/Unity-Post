using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityPost.WRE;
using WebRequestItem = UnityPost.WRE.WebRequestsEngine.WebRequestItem;

namespace UnityPost.Editor
{
    public class PostBuilderWindow : EditorWindow
    {
        private PostWindowSettings currentSettings
        {
            get
            {
                if (internalCurrentSettings == null)
                    internalCurrentSettings = new PostWindowSettings();
                return internalCurrentSettings;
            }
        }

        private PostWindowSettings internalCurrentSettings;

        [SerializeField] private VisualTreeAsset m_VisualTreeAsset;

        [MenuItem("Web Requests/Open Post Window")]
        public static void Show()
        {
            PostBuilderWindow wnd = GetWindow<PostBuilderWindow>();
            wnd.titleContent = new GUIContent("Post Window");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement uxmlRoot = m_VisualTreeAsset.Instantiate();

            //REQUEST TYPE 
            EnumField postDropDown = uxmlRoot.Q<EnumField>("the-uxml-field");
            postDropDown.Init(RequestTypes.GET);
            postDropDown.RegisterCallback<ChangeEvent<RequestTypes>>((evt) =>
            {
                currentSettings.currentType = evt.newValue;
            });

            //REQUEST URL
            TextField urlInput = uxmlRoot.Q<TextField>("url-input");
            urlInput.RegisterCallback<ChangeEvent<string>>((evt) => { currentSettings.url = evt.newValue; });

            //REQUEST URL
            Button sendButton = uxmlRoot.Q<Button>("send-button");
            sendButton.clicked += () => {SendRequest(currentSettings); };
            root.Add(uxmlRoot);
        }

        //send the request
        public async void SendRequest(PostWindowSettings settings)
        {
            //construct the web request
            WebRequestItem webRequestItem = new WebRequestItem(settings.url);
            webRequestItem.PrependWithDefaultURL = false;
            webRequestItem.SetPort(4000);

            string result = "";
            
            //Send request
            switch (settings.currentType)
            {
                case RequestTypes.GET:
                    result = await WebRequestsEngine.Active.GET(webRequestItem);
                    break;
                case RequestTypes.SET:
                    result = await WebRequestsEngine.Active.SET(webRequestItem);
                    break;
            }

            //display the result
            DisplayOutput(result);

        }
        
        public async void SendDemoRequest()
        {
            //1. Construct the web request
            WebRequestItem webRequestItem = new WebRequestItem("https://jsonplaceholder.typicode.com/posts/1");
            webRequestItem.PrependWithDefaultURL = false;
            
            //2. Add any needed parameters to the request
            webRequestItem.AddParameter("name", "namusanga");

            //3. Set a custom authentication
            webRequestItem.SetCustomAuthentication("LINSDC9474479HF394B8FW8742");
            
            string result = "";
            
            //4. Send request
            result = await WebRequestsEngine.Active.GET(webRequestItem);

            //5. Log the result 
            Debug.Log(result);
        }

        /// <summary>
        ///Will display the output in the output dialogue
        /// </summary>
        /// <param name="output"></param>
        private void DisplayOutput(string output)
        {
            //REQUEST URL
            TextField outputElement = rootVisualElement.Q<TextField>("result-output");
            outputElement.value = output;
        }
    }


    /// <summary>
    ///stores the current settings in the post window
    /// </summary>
    public class PostWindowSettings
    {
        public RequestTypes currentType = RequestTypes.GET;
        public string url;
    }
}