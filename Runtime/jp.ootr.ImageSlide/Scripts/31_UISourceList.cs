using JetBrains.Annotations;
using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class UISourceList : LogicPreloadUrls
    {
        [SerializeField] private InputField originalSourceNameInput;
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

        private readonly string[] _uiSourceListPrefix = { "UISourceList" };

        protected Toggle[] SourceToggles = new Toggle[0];
        protected InputField[] SourceInputs = new InputField[0];
        protected RawImage[] SourceIcons = new RawImage[0];


        protected void AddUrl([CanBeNull] VRCUrl url, URLType type, [CanBeNull] string options)
        {
            if (url == null || !url.ToString().IsValidUrl())
            {
                ConsoleError("invalid url", _uiSourceListPrefix);
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

        public void BuildSourceList([CanBeNull] [ItemCanBeNull] string[] sources = null,
            [CanBeNull] URLType[] options = null)
        {
            if (sources == null || options == null)
            {
                sources = definedSources;
                options = definedSourceTypes;
            }

            if (sources.Length != options.Length)
            {
                ConsoleError($"invalid source list length: {sources.Length} != {options.Length}", _uiSourceListPrefix);
                return;
            }

            Generate(sources, options);
        }

        private void Generate([CanBeNull] string[] sources, [CanBeNull] URLType[] types)
        {
            if (sources == null || types == null)
            {
                ConsoleError("sources or types is null", _uiSourceListPrefix);
                return;
            }
            
            ConsoleDebug($"generate source list: {sources.Length}", _uiSourceListPrefix);
            var currentLength = SourceToggles.Length;
            
            var children = rootSourceObject.transform.GetChildren();
            var baseObject = originalSourceNameInput.transform.parent.gameObject;
            
            if (currentLength < sources.Length)
            {
                SourceToggles = SourceToggles.Resize(sources.Length);
                SourceInputs = SourceInputs.Resize(sources.Length);
                SourceIcons = SourceIcons.Resize(sources.Length);
                
                for (var i = currentLength; i < sources.Length; i++)
                {
                    var obj = Instantiate(baseObject, rootSourceObject.transform);
                    obj.name = sources[i];
                    obj.SetActive(true);
                    obj.transform.SetSiblingIndex(i);
                    SourceToggles[i] = obj.transform.Find("__IDENTIFIER").GetComponent<Toggle>();
                    SourceInputs[i] = obj.transform.Find("CopyButton").GetComponent<InputField>();
                    SourceIcons[i] = obj.transform.Find("Image").GetComponent<RawImage>();
                }
                sourceTransform.ToListChildrenVertical(0, 0, true);
            }
            else if (currentLength > sources.Length)
            {
                SourceToggles = SourceToggles.Resize(sources.Length);
                SourceInputs = SourceInputs.Resize(sources.Length);
                SourceIcons = SourceIcons.Resize(sources.Length);
                
                for (var i = sources.Length; i < currentLength; i++)
                {
                    DestroyImmediate(children[i].gameObject);
                }
                sourceTransform.ToListChildrenVertical(0, 0, true);
            }
            
            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                if (SourceInputs[i].text != source) SourceInputs[i].text = source;
                var type = types[i];
                var texture = GetIcon(type);
                if (SourceIcons[i].texture != texture) SourceIcons[i].texture = texture;
                var obj = children[i];
                if (obj.name != source) obj.name = source;
            }
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
            var types = new URLType[Options.Length];
            for (var i = 0; i < Options.Length; i++) Options[i].ParseSourceOptions(out types[i]);
            BuildSourceList(Sources, types);
        }
    }
}
