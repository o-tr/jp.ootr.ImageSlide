using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UIThumbnails : UIErrorModal
    {
        private readonly int _slideListViewBaseGap = 16;
        private readonly int _slideListViewBasePadding = 16;

        private readonly int _slideListViewBaseThumbnailWidth = 375;

        [SerializeField] private Transform slideListViewRoot;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;
        [SerializeField] private ScrollRect slideListView;

        private Toggle[] _slideListToggles = new Toggle[0];
        private TextMeshProUGUI[] _slideListTexts = new TextMeshProUGUI[0];
        private RawImage[] _slideListThumbnails = new RawImage[0];
        private AspectRatioFitter[] _slideListFitters = new AspectRatioFitter[0];

        private string[] _slideListLoadedSources;
        private string[] _slideListLoadedFileNames;

        public override void UrlsUpdated()
        {
            base.UrlsUpdated();
            BuildSlideList();
        }

        public override void SeekModeChanged(SeekMode mode)
        {
            base.SeekModeChanged(mode);
            BuildSlideList();
        }

        protected override void LocalIndexUpdated(int index)
        {
            base.LocalIndexUpdated(index);
            var offset =
                (index * (_slideListViewBaseThumbnailWidth + _slideListViewBaseGap) - _slideListViewBaseGap +
                 _slideListViewBasePadding) / (slideListViewRoot.GetComponent<RectTransform>().rect.width -
                                               slideListView.GetComponent<RectTransform>().rect.width);
            slideListView.horizontalNormalizedPosition = Mathf.Max(Mathf.Min(offset, 1), 0);
        }

        public override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            _masterIndex = index;
            if (SeekMode == SeekMode.AllowViewedOnly)
            {
                if (_maxIndex < index)
                {
                    _maxIndex = index;
                    BuildSlideList();
                }
            }
            else if (SeekMode == SeekMode.AllowPreviousOnly)
            {
                _maxIndex = index;
                BuildSlideList();
            }

            if (_followMaster)
            {
                _localIndex = index;
                SeekTo(index);
            }
        }

        protected void BuildSlideList()
        {
            var slideCount = SeekMode == SeekMode.AllowPreviousOnly || SeekMode == SeekMode.AllowViewedOnly
                ? _maxIndex + 1
                : imageSlide.slideCount;
            var currentLength = _slideListToggles.Length;

            ConsoleDebug($"UISlide: {slideCount}, {currentLength}");

            if (currentLength < slideCount)
            {
                _slideListToggles = _slideListToggles.Resize(slideCount);
                _slideListThumbnails = _slideListThumbnails.Resize(slideCount);
                _slideListFitters = _slideListFitters.Resize(slideCount);
                _slideListTexts = _slideListTexts.Resize(slideCount);
                _thumbnailListLoadingSpinners = _thumbnailListLoadingSpinners.Resize(slideCount);

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
                    _thumbnailListLoadingSpinners[i] = obj.transform.Find("LoadingSpinner").gameObject;
                    _thumbnailListLoadingSpinners[i].SetActive(false);
                    ConsoleDebug(
                        $"{_slideListToggles[i]}, {_slideListThumbnails[i]}, {_slideListFitters[i]}, {_slideListTexts[i]}");
                }

                slideListViewRoot.ToListChildrenHorizontal(16, 16, true);
            }
            else if (currentLength > slideCount)
            {
                for (var i = currentLength - 1; i >= slideCount; i--)
                    DestroyImmediate(slideListViewRoot.GetChild(i).gameObject);
                _slideListToggles = _slideListToggles.Resize(slideCount);
                _slideListThumbnails = _slideListThumbnails.Resize(slideCount);
                _slideListFitters = _slideListFitters.Resize(slideCount);
                _slideListTexts = _slideListTexts.Resize(slideCount);
                _thumbnailListLoadingSpinners = _thumbnailListLoadingSpinners.Resize(slideCount);
                slideListViewRoot.ToListChildrenHorizontal(16, 16, true);
            }


            var loadSources = new string[slideCount];
            var loadFileNames = new string[slideCount];
            var index = 0;
            var sources = imageSlide.GetSources();

            for (var i = 0; i < imageSlide.FileNames.Length; i++)
            {
                var fileList = imageSlide.FileNames[i];
                var source = sources[i];
                for (var j = 0; j < fileList.Length; j++)
                {
                    if (index >= slideCount) break;
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

                    ConsoleDebug($"loading thumbnail: {source} {fileName}");

                    loadSources[index] = source;
                    loadFileNames[index] = fileName;

                    var label = (index + 1).ToString();
                    if (_slideListTexts[index].text != label) _slideListTexts[index].text = label;
                    index++;
                }
            }

            if (_slideListLoadedSources != null && _slideListLoadedFileNames != null)
                for (var i = 0; i < _slideListLoadedSources.Length; i++)
                {
                    var source = _slideListLoadedSources[i];
                    var fileName = _slideListLoadedFileNames[i];
                    if (source == null || fileName == null) continue;
                    ConsoleDebug($"releasing thumbnail: {source} {fileName}");
                    controller.CcReleaseTexture(source, fileName);
                }

            _slideListLoadedSources = loadSources;
            _slideListLoadedFileNames = loadFileNames;
            ConsoleDebug($"UISlide: loaded sources: {string.Join(",", _slideListLoadedFileNames)}");

            SeekTo(imageSlide.currentIndex);

            LoadThumbnailImages();
        }

        public void OnSlideListClicked()
        {
            if (!_slideListToggles.HasChecked(out var index) || SeekMode == SeekMode.DisallowAll) return;
            if ((SeekMode == SeekMode.AllowViewedOnly || SeekMode == SeekMode.AllowPreviousOnly) &&
                index >= _maxIndex) return;
            _followMaster = false;
            animator.SetBool(AnimatorFollowMaster, false);
            _localIndex = index;
            SeekTo(index);
        }

        private void LoadThumbnailImages()
        {
            ConsoleDebug($"LoadThumbnailImages: {string.Join(",", _slideListLoadedFileNames)}");
            for (var i = 0; i < _slideListLoadedSources.Length; i++)
            {
                var source = _slideListLoadedSources[i];
                var fileName = _slideListLoadedFileNames[i];

                if (source != null && fileName != null)
                {
                    ConsoleDebug($"loading thumbnail: {source} {fileName}");
                    controller.LoadFile(this, source, fileName);
                    _thumbnailListLoadingSpinners[i].SetActive(true);
                }
            }
        }

        public override void OnFileLoadSuccess(string sourceUrl, string fileUrl, string channel)
        {
            base.OnFileLoadSuccess(sourceUrl, fileUrl, channel);
            if (fileUrl == null) return;
            if (!_slideListLoadedFileNames.Has(fileUrl, out var index))
            {
                ConsoleDebug($"thumbnail image load success: {fileUrl} not found");
                return;
            }

            ConsoleDebug($"thumbnail image loaded: {fileUrl}");
            // エラー時も読み込み表示を解除
            if (index < _thumbnailListLoadingSpinners.Length)
            {
                _thumbnailListLoadingSpinners[index].SetActive(false);
            }

            var texture = controller.CcGetTexture(sourceUrl, fileUrl);
            if (texture == null)
            {
                ConsoleError($"Failed to get thumbnail texture for {sourceUrl}/{fileUrl}");
                return;
            }

            if (_slideListThumbnails.Length <= index)
            {
                ConsoleError($"thumbnail list index out of range: {index}");
                return;
            }

            _slideListThumbnails[index].texture = texture;
            _slideListFitters[index].aspectRatio = (float)texture.width / texture.height;
        }
    }
}
