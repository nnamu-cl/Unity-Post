using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Codice.Client.Commands;
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
        private ListView paramsList;

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

            //SEND BUTTON
            Button sendButton = uxmlRoot.Q<Button>("send-button");
            sendButton.clicked += () => { SendRequest(currentSettings); };
            root.Add(uxmlRoot);

            //PARAMETERS
            paramsList = uxmlRoot.Q<ListView>("header-list");
            paramsList.makeItem = GetParameterElement;
            paramsList.bindItem = BindItem;
            paramsList.itemsSource = currentSettings.headerParams;
            paramsList.selectionType = SelectionType.Multiple;
            paramsList.RefreshItems();

            //Add button
            Button paramAdd = uxmlRoot.Q<Button>("parameter-add");
            paramAdd.clicked += OnAddParamButton;
        }


        private void OnAddParamButton()
        {
            switch (currentSettings.currentParameterList)
            {
                case PostWindowSettings.ParameterList.Headers:
                    currentSettings.headerParams.Add(
                        new PostWindowSettings.Parameter($"Key {currentSettings.headerParams.Count}", "Value"));
                    break;
                case PostWindowSettings.ParameterList.Body:
                    currentSettings.bodyParams.Add(
                        new PostWindowSettings.Parameter($"Key {currentSettings.headerParams.Count}", "Value"));
                    break;
            }

            RefreshParamList();
        }

        private void RefreshParamList()
        {
            paramsList.itemsSource = currentSettings.headerParams;
            paramsList.RefreshItems();
        }

        private void BindItem(VisualElement e, int i)
        {
            TextField key = e.Q<TextField>("keyField");
            TextField value = e.Q<TextField>("valueField");

            key.value = currentSettings.headerParams[i].key;
            value.value = currentSettings.headerParams[i].value;

            key.RegisterCallback<ChangeEvent<string>>((s) =>
            {
                currentSettings.headerParams[i].key = s.newValue;
                RefreshParamList();
            });


            value.RegisterCallback<ChangeEvent<string>>((s) =>
            {
                currentSettings.headerParams[i].value = s.newValue;
                RefreshParamList();
            });
        }


        private VisualElement GetParameterElement()
        {
            VisualElement item = new VisualElement();

            //Key field
            TextField keyField = new TextField();
            keyField.name = "keyField";
            keyField.AddToClassList("horizontal-align-child");
            keyField.AddToClassList("parameter-field");
            keyField.value = "Key";

            //Key field
            TextField valueField = new TextField();
            valueField.name = "valueField";
            valueField.AddToClassList("horizontal-align-child");
            valueField.AddToClassList("parameter-field");
            valueField.value = "Value";

            item.Add(keyField);
            item.Add(valueField);
            item.AddToClassList("horizontal-align");
            return item;
        }

        //send the request
        public async void SendRequest(PostWindowSettings settings)
        {
            //construct the web request
            WebRequestItem webRequestItem = new WebRequestItem(settings.url);
            webRequestItem.PrependWithDefaultURL = false;

            string result = await WebRequestsEngine.Active.GET(webRequestItem);
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
    public class PostWindowSettings : ScriptableObject
    {
        public RequestTypes currentType = RequestTypes.GET;
        public string url;
        public ParameterList currentParameterList = ParameterList.Headers;
        public List<Parameter> headerParams = new List<Parameter>();
        public List<Parameter> bodyParams = new List<Parameter>();


        public enum ParameterList
        {
            Headers,
            Body
        }

        public class Parameter
        {
            public Parameter(string _key, string _value)
            {
                key = _key;
                value = _value;
            }

            public string key;
            public string value;
        }
    }
}