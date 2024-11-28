using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class UISlide : UIDeviceList
    {
        private const int SlideListViewBaseThumbnailWidth = 375;
        private const int SlideListViewBaseGap = 16;
        private const int SlideListViewBasePadding = 16;
        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;

        [SerializeField] private RawImage slideNextView;
        [SerializeField] private AspectRatioFitter slideNextViewFitter;

        [SerializeField] private TextMeshProUGUI slideMainViewNote;

        [SerializeField] private Transform slideListViewRoot;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;

        [SerializeField] private TextMeshProUGUI slideCountText;

        [SerializeField] private ScrollRect slideListView;

        [SerializeField] private Texture2D splashScreen;
        [SerializeField] private Texture2D blankTexture;

        private readonly int _animatorSplash = Animator.StringToHash("Splash");

        private Toggle[] _slideListToggles;
        
        private string[] _slideListLoadedSources;
        private string[] _slideListLoadedFileNames;
        private string _mainLoadedSource;
        private string _mainLoadedFileName;
        private string _nextLoadedSource;
        private string _nextLoadedFileName;

        public void SeekToNext()
        {
            if (slideCount <= currentIndex + 1) return;
            SeekTo(currentIndex + 1);
        }

        public void SeekToPrevious()
        {
            if (currentIndex <= 0) return;
            SeekTo(currentIndex - 1);
        }

        public void OnSlideListClicked()
        {
            if (!_slideListToggles.HasChecked(out var index)) return;
            SeekTo(index);
        }

        public void SeekToStart()
        {
            if (currentIndex == 0) return;
            SeekTo(0);
        }

        public void SeekToEnd()
        {
            if (currentIndex == slideCount - 1) return;
            SeekTo(slideCount - 1);
        }

        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();
            slideListViewRoot.ClearChildren();
            BuildSlideList();
        }

        private void BuildSlideList()
        {
            _slideListToggles = new Toggle[slideCount];
            var index = 0;
            if (FileNames.Length != Sources.Length) return;
            var loadSources = new string[slideCount];
            var loadFileNames = new string[slideCount];
            for (var i = 0; i < FileNames.Length; i++)
            {
                var source = Sources[i];
                var fileList = FileNames[i];
                for (var j = 0; j < fileList.Length; j++)
                {
                    var fileName = fileList[j];
                    ConsoleInfo($"load slide list: {source} / {fileName}");
                    var texture = controller.CcGetTexture(source, fileName);
                    if (texture != null)
                    {
                        slideListViewBaseThumbnail.texture = texture;
                        slideListViewBaseFitter.aspectRatio = (float)texture.width / texture.height;
                        loadSources[index] = source;
                        loadFileNames[index] = fileName;
                    }
                    slideListViewBaseText.text = (index + 1).ToString();
                    var obj = Instantiate(slideListViewBase, slideListViewRoot);
                    obj.name = fileName;
                    obj.SetActive(true);
                    obj.transform.SetSiblingIndex(index);
                    _slideListToggles[index] = obj.transform.Find("__IDENTIFIER").GetComponent<Toggle>();
                    index++;
                }
            }

            if (_slideListLoadedSources != null && _slideListLoadedFileNames != null)
            {
                for (var i = 0; i < _slideListLoadedSources.Length; i++)
                {
                    var source = _slideListLoadedSources[i];
                    var fileName = _slideListLoadedFileNames[i];
                    if (source == null || fileName == null) continue;
                    controller.CcReleaseTexture(source, fileName);
                }
            }
            
            _slideListLoadedSources = loadSources;
            _slideListLoadedFileNames = loadFileNames;

            slideListViewRoot.ToListChildrenHorizontal(16, 16, true);
            SetTexture(currentIndex);
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            var offset =
                (index * (SlideListViewBaseThumbnailWidth + SlideListViewBaseGap) - SlideListViewBaseGap +
                 SlideListViewBasePadding) / (slideListViewRoot.GetComponent<RectTransform>().rect.width -
                                              slideListView.GetComponent<RectTransform>().rect.width);
            slideListView.horizontalNormalizedPosition = Mathf.Max(Mathf.Min(offset, 1), 0);
            SetTexture(index);
        }

        private void SetTexture(int index)
        {
            slideCountText.text = $"{index + 1} / {slideCount}";
            ConsoleDebug($"slide index updated: {index} / {slideCount}");
            
            var texture = TryGetTextureByIndex(index, out var source, out var fileName);
            animator.SetBool(_animatorSplash, texture == null || slideCount == 0);
            
            if (texture != null)
            {
                slideMainView.texture = texture;
                slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
                var metadata = controller.CcGetMetadata(source, fileName);
                SetNote(metadata);
                CastToScreens(source, fileName);
            }

            if (_mainLoadedSource != null && _mainLoadedFileName != null)
            {
                ConsoleInfo($"unload main: {_mainLoadedSource} / {_mainLoadedFileName}");
                controller.CcReleaseTexture(_mainLoadedSource, _mainLoadedFileName);
                _mainLoadedSource = null;
                _mainLoadedFileName = null;
            }
            _mainLoadedSource = source;
            _mainLoadedFileName = fileName;

            SetNextTexture(index);
        }
        
        private void CastToScreens(string source, string fileName)
        {
            if (!Networking.IsOwner(gameObject)) return;
            foreach (var device in devices)
            {
                if (device == null || !device.IsCastableDevice() ||
                    !deviceSelectedUuids.Has(device.deviceUuid)) continue;
                device.LoadImage(source, fileName);
            }
        }

        private void SetNextTexture(int index)
        {
            var nextIndex = index + 1;
            var nextTexture = TryGetTextureByIndex(nextIndex, out var source, out var fileName);

            if (nextTexture != null)
            {
                slideNextView.texture = nextTexture;
                slideNextViewFitter.aspectRatio = (float)nextTexture.width / nextTexture.height;
            }
            else
            {
                slideNextView.texture = blankTexture;
                slideNextViewFitter.aspectRatio = (float)blankTexture.width / blankTexture.height;
            }
            
            if (_nextLoadedSource != null && _nextLoadedFileName != null)
            {
                ConsoleInfo($"unload next: {_nextLoadedSource} / {_nextLoadedFileName}");
                controller.CcReleaseTexture(_nextLoadedSource, _nextLoadedFileName);
            }
            _nextLoadedSource = source;
            _nextLoadedFileName = fileName;
        }

        private Texture2D TryGetTextureByIndex(int index, out string source, out string fileName)
        {
            if (!FileNames.GetByIndex(index, out var sourceIndex, out var fileIndex))
            {
                source = null;
                fileName = null;
                return null;
            }
            source = Sources[sourceIndex];
            fileName = FileNames[sourceIndex][fileIndex];
            ConsoleInfo($"load texture: {source} / {fileName}");
            return controller.CcGetTexture(source, fileName);
        }

        private void SetNote(Metadata metadata)
        {
            if (metadata == null) return;
            var extensions = metadata.GetExtensions();
            if (extensions == null) return;
            if (extensions.TryGetValue("note", TokenType.String, out var note))
                slideMainViewNote.text = note.ToString();
            else
                slideMainViewNote.text = "";
        }
        
        public DeviceController GetController()
        {
            return controller;
        }
    }
}
