using JetBrains.Annotations;
using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UIThumbnails : UISlide
    {
        private const int ThumbnailListViewBaseThumbnailWidth = 375;
        private const int ThumbnailListViewBaseGap = 16;
        private const int ThumbnailListViewBasePadding = 16;
        [SerializeField] private ScrollRect thumbnailListView;
        [SerializeField] private Transform thumbnailListViewRoot;
        [SerializeField] private RectTransform thumbnailListViewRootRectTransform;
        [SerializeField] private GameObject thumbnailListViewBase;
        [SerializeField] private RawImage thumbnailListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter thumbnailListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI thumbnailListViewBaseText;
        [SerializeField] private RectTransform thumbnailListViewRectTransform;
        [SerializeField] internal bool enableThumbnails = true;
        [NotNull] private AspectRatioFitter[] _thumbnailListFitters = new AspectRatioFitter[0];
        [NotNull][ItemCanBeNull] private string[] _thumbnailListLoadedFileNames = new string[0];

        [NotNull][ItemCanBeNull] private string[] _thumbnailListLoadedSources = new string[0];
        [NotNull] private TextMeshProUGUI[] _thumbnailListTexts = new TextMeshProUGUI[0];
        [NotNull] private RawImage[] _thumbnailListThumbnails = new RawImage[0];
        [NotNull] private Toggle[] _thumbnailListToggles = new Toggle[0];
        [NotNull] private GameObject[] _thumbnailListLoadingSpinners = new GameObject[0];

        public override void InitController()
        {
            base.InitController();
            animator.SetBool(Animator.StringToHash("IsThumbnailsEnabled"), enableThumbnails);
        }


        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();
            if (!enableThumbnails) return;
            BuildThumbnailList();
            LoadThumbnailImages();
        }

        public void OnSlideListClicked()
        {
            if (!_thumbnailListToggles.HasChecked(out var index)) return;
            SeekTo(index);
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            if (!enableThumbnails) return;

            // サムネイルの左端位置
            var thumbnailLeftPosition = index * (ThumbnailListViewBaseThumbnailWidth + ThumbnailListViewBaseGap) - ThumbnailListViewBaseGap + ThumbnailListViewBasePadding;
            // サムネイルの幅
            var thumbnailWidth = ThumbnailListViewBaseThumbnailWidth;
            // スクロールビューの幅
            var scrollViewWidth = thumbnailListViewRectTransform.rect.width;
            // 中央に来る位置（サムネイルの中心からスクロールビューの半分を引く）
            var centerPosition = thumbnailLeftPosition + thumbnailWidth / 2.0f - scrollViewWidth / 2.0f;
            // スクロール可能な範囲
            var scrollableRange = thumbnailListViewRootRectTransform.rect.width - scrollViewWidth;
            // 正規化された位置（0-1の範囲にクランプ）
            var offset = scrollableRange > 0 ? Mathf.Clamp01(centerPosition / scrollableRange) : 0;
            thumbnailListView.horizontalNormalizedPosition = offset;
        }

        private void BuildThumbnailList()
        {
            ConsoleDebug($"build thumbnail list. current: {_thumbnailListToggles.Length} / target: {slideCount}");
            var currentLength = _thumbnailListToggles.Length;

            if (FileNames.Length != Sources.Length)
            {
                ConsoleError("FileNames and Sources length mismatch");
                return;
            }

            if (currentLength < slideCount)
            {
                _thumbnailListToggles = _thumbnailListToggles.Resize(slideCount);
                _thumbnailListThumbnails = _thumbnailListThumbnails.Resize(slideCount);
                _thumbnailListFitters = _thumbnailListFitters.Resize(slideCount);
                _thumbnailListTexts = _thumbnailListTexts.Resize(slideCount);
                _thumbnailListLoadingSpinners = _thumbnailListLoadingSpinners.Resize(slideCount);

                for (var i = currentLength; i < slideCount; i++)
                {
                    var obj = Instantiate(thumbnailListViewBase, thumbnailListViewRoot);
                    obj.name = i.ToString();
                    obj.SetActive(true);
                    obj.transform.SetSiblingIndex(i);
                    _thumbnailListToggles[i] = obj.transform.Find("__IDENTIFIER").GetComponent<Toggle>();
                    _thumbnailListThumbnails[i] = obj.transform.Find("GameObject/RawImage").GetComponent<RawImage>();
                    _thumbnailListFitters[i] = obj.transform.Find("GameObject/RawImage").GetComponent<AspectRatioFitter>();
                    _thumbnailListTexts[i] = obj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                    _thumbnailListLoadingSpinners[i] = obj.transform.Find("LoadingSpinner").gameObject;
                    _thumbnailListLoadingSpinners[i].SetActive(false);
                }

                thumbnailListViewRoot.ToListChildrenHorizontal(16, 16, true);
            }
            else if (currentLength > slideCount)
            {
                for (var i = slideCount; i < currentLength; i++) DestroyImmediate(_thumbnailListToggles[i].transform.parent.gameObject);

                _thumbnailListToggles = _thumbnailListToggles.Resize(slideCount);
                _thumbnailListThumbnails = _thumbnailListThumbnails.Resize(slideCount);
                _thumbnailListFitters = _thumbnailListFitters.Resize(slideCount);
                _thumbnailListTexts = _thumbnailListTexts.Resize(slideCount);
                _thumbnailListLoadingSpinners = _thumbnailListLoadingSpinners.Resize(slideCount);
                thumbnailListViewRoot.ToListChildrenHorizontal(16, 16, true);
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
                    if (_thumbnailListLoadedSources.Length > index && _thumbnailListLoadedSources[index] == source &&
                        _thumbnailListLoadedFileNames.Length > index && _thumbnailListLoadedFileNames[index] == fileName)
                    {
                        _thumbnailListLoadedSources[index] = null;
                        _thumbnailListLoadedFileNames[index] = null;
                        loadSources[index] = source;
                        loadFileNames[index] = fileName;
                        index++;
                        continue;
                    }

                    ConsoleDebug($"load thumbnail list: {source} / {fileName}");
                    loadSources[index] = source;
                    loadFileNames[index] = fileName;

                    var label = (index + 1).ToString();
                    _thumbnailListTexts[index].text = label;
                    index++;
                }
            }

            for (var i = 0; i < _thumbnailListLoadedSources.Length; i++)
            {
                var source = _thumbnailListLoadedSources[i];
                var fileName = _thumbnailListLoadedFileNames[i];
                if (source == null || fileName == null) continue;
                ConsoleDebug($"unload thumbnail list: {source} / {fileName}");
                controller.CcReleaseTexture(source, fileName);
            }

            _thumbnailListLoadedSources = loadSources;
            _thumbnailListLoadedFileNames = loadFileNames;

        }

        private void LoadThumbnailImages()
        {
            for (var i = 0; i < FlatFileNames.Length; i++)
            {
                var source = FlatSources[i];
                var fileName = FlatFileNames[i];
                controller.LoadFile(this, source, fileName);
                _thumbnailListLoadingSpinners[i].SetActive(true);
            }
        }

        public override void OnFileLoadSuccess(string sourceUrl, string fileUrl, string channel)
        {
            base.OnFileLoadSuccess(sourceUrl, fileUrl, channel);
            if (!enableThumbnails) return;
            if (fileUrl == null) return;
            if (!FlatFileNames.Has(fileUrl, out var index)) return;
            ConsoleDebug($"thumbnail image loaded: {fileUrl}");
            var source = FlatSources[index];
            var texture = controller.CcGetTexture(source, fileUrl);
            if (texture == null) return;
            if (_thumbnailListThumbnails.Length <= index)
            {
                ConsoleError($"thumbnail list index out of range: {index}");
                return;
            }
            _thumbnailListThumbnails[index].texture = texture;
            _thumbnailListFitters[index].aspectRatio = (float)texture.width / texture.height;
            _thumbnailListLoadingSpinners[index].SetActive(false);
        }
    }
}
