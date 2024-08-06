using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class UISourceList : LogicQueue
    {
        [SerializeField] public string[] definedSources;
        [SerializeField] public string[] definedSourceOptions;
        
        [SerializeField] private TMP_InputField originalSourceNameInput;
        [SerializeField] private RawImage originalSourceIcon;
        [SerializeField] private Transform sourceTransform;
        [SerializeField] private GameObject rootSourceObject;

        [SerializeField] private Texture2D imageIcon;
        [SerializeField] private Texture2D textZipIcon;
        [SerializeField] private Texture2D videoIcon;

        [SerializeField] private VRCUrlInputField sourceImageUrlInput;
        [SerializeField] private VRCUrlInputField sourceTextZipUrlInput;
        [SerializeField] protected VRCUrlInputField sourceVideoUrlInput;
        
        [SerializeField] protected Slider sourceVideoOffsetSlider;
        [SerializeField] protected TMP_InputField sourceVideoOffsetInput;
        [SerializeField] protected Slider sourceVideoIntervalSlider;
        [SerializeField] protected TMP_InputField sourceVideoIntervalInput;
        
        protected Toggle[] SourceToggles;
        
        
        protected void AddUrl(VRCUrl url, URLType type, string options)
        {
            if (definedSources.Has(url.ToString()))
            {
                ShowErrorModal("Error", "This source is already added.");
                return;
            }
            controller.UsAddUrl(url);
            AddSourceQueue(url.ToString(), options);
        }

        protected void GetUrl(out VRCUrl url, out URLType type, out string options)
        {
            var imageUrl = sourceImageUrlInput.GetUrl();
            var textZipUrl = sourceTextZipUrlInput.GetUrl();

            ResetInputs();
            
            if (!imageUrl.ToString().IsNullOrEmpty())
            {
                url = imageUrl;
                type = URLType.Image;
                options = UrlUtil.BuildSourceOptions(type, 0, 0);
                return;
            }
            url = textZipUrl;
            type = URLType.TextZip;
            options = UrlUtil.BuildSourceOptions(type, 0, 0);
        }

        protected void ResetInputs()
        {
            sourceImageUrlInput.SetUrl(VRCUrl.Empty);
            sourceTextZipUrlInput.SetUrl(VRCUrl.Empty);
            sourceVideoUrlInput.SetUrl(VRCUrl.Empty);
            sourceVideoIntervalInput.text = "1";
            sourceVideoIntervalSlider.value = 1f;
            sourceVideoOffsetInput.text = "0.5";
            sourceVideoOffsetSlider.value = 0.5f;
        }
        
        public void BuildSourceList(string[] sources = null, string[] options = null)
        {
            if (sources == null || options == null)
            {
                sources = definedSources;
                options = definedSourceOptions;
            }
            if (sources.Length != options.Length) return;
            CleanUp();
            Generate(sources, options);
        }

        private void CleanUp()
        {
            var list = new GameObject[rootSourceObject.transform.childCount];
            var count = 0;
            foreach (Transform child in rootSourceObject.transform)
            {
                if (child.gameObject.name.StartsWith("_")) continue;
                list[count++] = child.gameObject; 
            }
            for(var i = 0; i < count; i++)
            {
                DestroyImmediate(list[i]);
            }
        }

        private void Generate(string[] sources, string[] options)
        {
            ConsoleDebug($"Generate {sources.Length}");
            var children = rootSourceObject.transform.GetChildren();
            var baseObject = originalSourceNameInput.transform.parent.gameObject;
            SourceToggles = new Toggle[sources.Length];
            
            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                var option = options[i];
                originalSourceNameInput.text = source;
                option.ParseSourceOptions(out var type);
                originalSourceIcon.texture = GetIcon(type);
                var obj = Instantiate(baseObject, rootSourceObject.transform);
                obj.name = source;
                obj.SetActive(true);
                obj.transform.SetSiblingIndex(i);
                SourceToggles[i] = obj.transform.Find("__IDENTIFIER").GetComponent<Toggle>();
            }

            for (int i = 0; i < children.Length; i++)
            {
                children[i].SetSiblingIndex(sources.Length + i);
            }
            
            sourceTransform.ToListChildrenVertical(0,0,true);
        }

        private Texture2D GetIcon(URLType type)
        {
            switch (type)
            {
                case URLType.Video:
                    return videoIcon;
                case URLType.TextZip:
                    return textZipIcon;
                default:
                    return imageIcon;
            }
        }

        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();
            ConsoleDebug($"[UrlsUpdated] {_sources.Length}, {_options.Length}");
            BuildSourceList(_sources,_options);
        }
    }
}