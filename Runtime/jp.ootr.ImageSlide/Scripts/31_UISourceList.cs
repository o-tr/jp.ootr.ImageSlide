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
        [SerializeField] public string[] definedSources = new string[0];
        [SerializeField] public URLType[] definedSourceTypes = new URLType[0];
        [SerializeField] public float[] definedSourceOffsets = new float[0];
        [SerializeField] public float[] definedSourceIntervals = new float[0];

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

        private readonly string[] _uiSourceListPrefix = { "UISourceList" };

        protected Toggle[] SourceToggles;

        protected void AddUrl(VRCUrl url, URLType type, string options)
        {
            if (definedSources.Has(url.ToString()))
            {
                ShowErrorModal("Error", "This source is already added.");
                ConsoleWarn($"this source is already added: {url}", _uiSourceListPrefix);
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

        public void BuildSourceList(string[] sources = null, URLType[] options = null)
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

            rootSourceObject.transform.ClearChildren();
            Generate(sources, options);
        }

        private void Generate(string[] sources, URLType[] types)
        {
            ConsoleDebug($"generate source list: {sources.Length}", _uiSourceListPrefix);
            var children = rootSourceObject.transform.GetChildren();
            var baseObject = originalSourceNameInput.transform.parent.gameObject;
            SourceToggles = new Toggle[sources.Length];

            for (var i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                originalSourceNameInput.text = source;
                var type = types[i];
                originalSourceIcon.texture = GetIcon(type);
                var obj = Instantiate(baseObject, rootSourceObject.transform);
                obj.name = source;
                obj.SetActive(true);
                obj.transform.SetSiblingIndex(i);
                SourceToggles[i] = obj.transform.Find("__IDENTIFIER").GetComponent<Toggle>();
            }

            for (var i = 0; i < children.Length; i++) children[i].SetSiblingIndex(sources.Length + i);

            sourceTransform.ToListChildrenVertical(0, 0, true);
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
