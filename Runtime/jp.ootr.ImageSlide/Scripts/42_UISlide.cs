using JetBrains.Annotations;
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
        [SerializeField] private RectTransform slideListViewRootRectTransform;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;

        [SerializeField] private TextMeshProUGUI slideCountText;

        [SerializeField] private ScrollRect slideListView;
        [SerializeField] private RectTransform slideListViewRectTransform;

        [SerializeField] private Texture2D splashScreen;
        [SerializeField] private Texture2D blankTexture;

        private readonly int _animatorSplash = Animator.StringToHash("Splash");

        [NotNull] private Toggle[] _slideListToggles = new Toggle[0];
        [NotNull] private RawImage[] _slideListThumbnails = new RawImage[0];
        [NotNull]private AspectRatioFitter[] _slideListFitters = new AspectRatioFitter[0];
        [NotNull]private TextMeshProUGUI[] _slideListTexts = new TextMeshProUGUI[0];
        
        [NotNull][ItemCanBeNull]private string[] _slideListLoadedSources = new string[0];
        [NotNull][ItemCanBeNull]private string[] _slideListLoadedFileNames = new string[0];
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
            var currentLength = _slideListToggles.Length;
            
            if (FileNames.Length != Sources.Length)
            {
                ConsoleError("FileNames and Sources length mismatch");
                return;
            }

            if (currentLength < slideCount)
            {
                _slideListToggles = _slideListToggles.Resize(slideCount);
                _slideListThumbnails = _slideListThumbnails.Resize(slideCount);
                _slideListFitters = _slideListFitters.Resize(slideCount);
                _slideListTexts = _slideListTexts.Resize(slideCount);
                
                for (var i = currentLength; i < slideCount; i++)
                {
                    var obj = Instantiate(slideListViewBase, slideListViewRoot);
                    obj.name = i.ToString();
                    obj.SetActive(true);
                    obj.transform.SetSiblingIndex(i);
                    _slideListToggles[i] = obj.transform.Find("__IDENTIFIER").GetComponent<Toggle>();
                    _slideListThumbnails[i] = obj.transform.Find("GameObject/RawImage").GetComponent<RawImage>();
                    _slideListFitters[i] = obj.transform.Find("GameObject/RawImage").GetComponent<AspectRatioFitter>();
                    _slideListTexts[i] = obj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                }
                slideListViewRoot.ToListChildrenHorizontal(16, 16, true);
            }
            else if (currentLength > slideCount)
            {
                _slideListToggles = _slideListToggles.Resize(slideCount);
                _slideListThumbnails = _slideListThumbnails.Resize(slideCount);
                _slideListFitters = _slideListFitters.Resize(slideCount);
                _slideListTexts = _slideListTexts.Resize(slideCount);
                
                for (var i = slideCount; i < currentLength; i++)
                {
                    DestroyImmediate(_slideListToggles[i].gameObject);
                }
                slideListViewRoot.ToListChildrenHorizontal(16, 16, true);
            }
            
            var loadSources = new string[slideCount];
            var loadFileNames = new string[slideCount];
            var index = 0;
            
            for (var i = 0; i < FileNames.Length; i++)
            {
                var source = Sources[i];
                var fileList = FileNames[i];
                for (var j = 0; j < fileList.Length; j++)
                {
                    var fileName = fileList[j];
                    if (_slideListLoadedSources.Length > index && _slideListLoadedSources[index] == source &&
                        _slideListLoadedFileNames.Length > index && _slideListLoadedFileNames[index] == fileName)
                    {
                        _slideListLoadedSources[index] = null;
                        _slideListLoadedFileNames[index] = null;
                        loadSources[index] = source;
                        loadFileNames[index] = fileName;
                        index++;
                        continue;
                    }
                    ConsoleDebug($"load slide list: {source} / {fileName}");
                    var texture = controller.CcGetTexture(source, fileName);
                    if (texture != null)
                    {
                        loadSources[index] = source;
                        loadFileNames[index] = fileName;
                        _slideListThumbnails[index].texture = texture;
                        _slideListFitters[index].aspectRatio = (float)texture.width / texture.height;
                    }
                    var label = (index + 1).ToString();
                    _slideListTexts[index].text = label;
                    index++;
                }
            }
            
            for (var i = 0; i < _slideListLoadedSources.Length; i++)
            {
                var source = _slideListLoadedSources[i];
                var fileName = _slideListLoadedFileNames[i];
                if (source == null || fileName == null) continue;
                ConsoleDebug($"unload slide list: {source} / {fileName}");
                controller.CcReleaseTexture(source, fileName);
            }
            
            _slideListLoadedSources = loadSources;
            _slideListLoadedFileNames = loadFileNames;
            
            SetTexture(currentIndex);
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            var offset =
                (index * (SlideListViewBaseThumbnailWidth + SlideListViewBaseGap) - SlideListViewBaseGap +
                 SlideListViewBasePadding) / (slideListViewRootRectTransform.rect.width -
                                              slideListViewRectTransform.rect.width);
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
                if (texture != slideMainView.texture)
                {
                    slideMainView.texture = texture;
                    slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
                }
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
                if (nextTexture != slideNextView.texture)
                {
                    slideNextView.texture = nextTexture;
                    slideNextViewFitter.aspectRatio = (float)nextTexture.width / nextTexture.height;
                }
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
