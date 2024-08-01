using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class UISourceList : UIDeviceList
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
        [SerializeField] private VRCUrlInputField sourceVideoUrlInput;
        
        [SerializeField] private Slider sourceVideoOffsetSlider;
        [SerializeField] private TMP_InputField sourceVideoOffsetInput;
        [SerializeField] private Slider sourceVideoIntervalSlider;
        [SerializeField] private TMP_InputField sourceVideoIntervalInput;
        
        private Toggle[] _sourceToggles;
        
        public void OnSourceEndEdit()
        {
            GetUrl(out var url, out var type, out var options);
            if (url.ToString().IsNullOrEmpty()) return;
            AddUrl(url, type, options);
            ResetInputs();
            BuildSourceList();
        }
        
        public void OnVideoSourceEndEdit()
        {
            var source = sourceVideoUrlInput.GetUrl();
            if (source.ToString().IsNullOrEmpty()) return;
            sourceVideoUrlInput.SetUrl(VRCUrl.Empty);
            var options = UrlUtil.BuildSourceOptions(URLType.Video, sourceVideoOffsetSlider.value, sourceVideoIntervalSlider.value);
            AddUrl(source, URLType.Video, options);
            ResetInputs();
            OnCloseOverlay();
            BuildSourceList();
        }
        
        public void OnVideoOffsetSliderChange()
        {
            var value = Mathf.Round(sourceVideoOffsetSlider.value * 10) / 10;
            sourceVideoOffsetInput.text = value.ToString();
            sourceVideoOffsetSlider.value = value;
        }
        
        public void OnVideoOffsetEndEdit()
        {
            sourceVideoOffsetSlider.value = float.Parse(sourceVideoOffsetInput.text);
        }
        
        public void OnVideoIntervalSliderChange()
        {
            var value = Mathf.Round(sourceVideoIntervalSlider.value * 10) / 10;
            sourceVideoIntervalInput.text = value.ToString();
            sourceVideoIntervalSlider.value = value;
        }
        
        public void OnVideoIntervalEndEdit()
        {
            sourceVideoIntervalSlider.value = float.Parse(sourceVideoIntervalInput.text);
        }

        public void OnSourceDelete()
        {
            if (!_sourceToggles.HasChecked(out var index)) return;
            RemoveSourceQueue(_sourceToggles[index].name);
        }
        
        private void AddUrl(VRCUrl url, URLType type, string options)
        {
            if (definedSources.Has(url.ToString()))
            {
                ShowErrorModal("Error", "This source is already added.");
                return;
            }
            controller.UsAddUrl(url);
            AddSourceQueue(url.ToString(), options);
        }

        private void GetUrl(out VRCUrl url, out URLType type, out string options)
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

        private void ResetInputs()
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
            var children = rootSourceObject.transform.GetChildren();
            var baseObject = originalSourceNameInput.transform.parent.gameObject;
            _sourceToggles = new Toggle[sources.Length];
            
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
                _sourceToggles[i] = obj.GetComponent<Toggle>();
            }

            for (int i = 0; i < children.Length; i++)
            {
                children[i].SetSiblingIndex(sources.Length + i);
            }
            
            sourceTransform.ToListChildren();
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