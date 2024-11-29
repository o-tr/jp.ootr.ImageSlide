using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UISlide : UISeekMode
    {
        [SerializeField] internal ImageSlide imageSlide;

        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;

        [SerializeField] private Transform slideListViewRoot;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;

        [SerializeField] private ScrollRect slideListView;

        [SerializeField] private Texture2D blankTexture;

        private readonly int _animatorFollowMaster = Animator.StringToHash("FollowMaster");
        private readonly int _slideListViewBaseGap = 16;
        private readonly int _slideListViewBasePadding = 16;

        private readonly int _slideListViewBaseThumbnailWidth = 375;

        private bool _followMaster = true;
        private int _localIndex;
        private int _masterIndex;

        private int _maxIndex;
        private Toggle[] _slideListToggles;
        
        private string[] _slideListLoadedSources;
        private string[] _slideListLoadedFileNames;
        
        private string _mainLoadedSource;
        private string _mainLoadedFileName;

        public override void SeekModeChanged(SeekMode mode)
        {
            base.SeekModeChanged(mode);
            RebuildSlideList();
        }

        public override void UrlsUpdated()
        {
            base.UrlsUpdated();
            RebuildSlideList();
        }

        private void RebuildSlideList()
        {
            slideListViewRoot.ClearChildren();
            BuildSlideList();
        }

        private void BuildSlideList()
        {
            var slideCount = SeekMode == SeekMode.AllowPreviousOnly || SeekMode == SeekMode.AllowViewedOnly
                ? _maxIndex + 1
                : imageSlide.slideCount;

            _slideListToggles = new Toggle[slideCount];
            var loadSources = new string[slideCount];
            var loadFileNames = new string[slideCount];
            var controller = imageSlide.GetController();
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
                    
                    ConsoleInfo($"loading thumbnail: {source} {fileName}");
                    var texture = controller.CcGetTexture(source, fileName);
                    
                    if (texture != null)
                    {
                        loadSources[index] = source;
                        loadFileNames[index] = fileName;
                        
                        slideListViewBaseThumbnail.texture = texture;
                        slideListViewBaseFitter.aspectRatio = (float)texture.width / texture.height;
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
                    ConsoleInfo($"releasing thumbnail: {source} {fileName}");
                    controller.CcReleaseTexture(source, fileName);
                }
            }
            
            _slideListLoadedSources = loadSources;
            _slideListLoadedFileNames = loadFileNames;

            slideListViewRoot.ToListChildrenHorizontal(16, 16, true);
            SetTexture(imageSlide.currentIndex);
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
                    RebuildSlideList();
                }
            }
            else if (SeekMode == SeekMode.AllowPreviousOnly)
            {
                _maxIndex = index;
                RebuildSlideList();
            }

            if (_followMaster)
            {
                _localIndex = index;
                SeekTo(index);
            }
        }

        public void OnSlideListClicked()
        {
            if (!_slideListToggles.HasChecked(out var index) || SeekMode == SeekMode.DisallowAll) return;
            if ((SeekMode == SeekMode.AllowViewedOnly || SeekMode == SeekMode.AllowPreviousOnly) &&
                index >= _maxIndex) return;
            _followMaster = false;
            animator.SetBool(_animatorFollowMaster, false);
            _localIndex = index;
            SeekTo(index);
        }

        public void SeekToNext()
        {
            if (imageSlide.slideCount <= _localIndex + 1 || SeekMode == SeekMode.DisallowAll) return;
            if ((SeekMode == SeekMode.AllowViewedOnly || SeekMode == SeekMode.AllowPreviousOnly) &&
                _localIndex + 1 > _maxIndex) return;
            _followMaster = false;
            animator.SetBool(_animatorFollowMaster, false);
            SeekTo(++_localIndex);
        }

        public void SeekToPrevious()
        {
            if (_localIndex <= 0 || SeekMode == SeekMode.DisallowAll) return;
            _followMaster = false;
            animator.SetBool(_animatorFollowMaster, false);
            SeekTo(--_localIndex);
        }

        public void FollowMaster()
        {
            _followMaster = true;
            animator.SetBool(_animatorFollowMaster, true);
            _localIndex = _masterIndex;
            SeekTo(_masterIndex);
        }

        private void SeekTo(int index)
        {
            var offset =
                (index * (_slideListViewBaseThumbnailWidth + _slideListViewBaseGap) - _slideListViewBaseGap +
                 _slideListViewBasePadding) / (slideListViewRoot.GetComponent<RectTransform>().rect.width -
                                               slideListView.GetComponent<RectTransform>().rect.width);
            slideListView.horizontalNormalizedPosition = Mathf.Max(Mathf.Min(offset, 1), 0);
            SetTexture(index);
        }

        private void SetTexture(int index)
        {
            if (!imageSlide.FileNames.GetByIndex(index, out var sourceIndex, out var fileIndex)) return;
            var source = imageSlide.GetSources()[sourceIndex];
            var fileName = imageSlide.FileNames[sourceIndex][fileIndex];
            var controller = imageSlide.GetController();
            ConsoleInfo($"load main: {source} / {fileName}");
            var texture = controller.CcGetTexture(source, fileName);
            
            if (_mainLoadedSource != null && _mainLoadedFileName != null)
            {
                ConsoleInfo($"unload main: {_mainLoadedSource} / {_mainLoadedFileName}");
                controller.CcReleaseTexture(_mainLoadedSource, _mainLoadedFileName);
            }
            
            if (texture == null)
            {
                slideMainView.texture = blankTexture;
                _mainLoadedSource = null;
                _mainLoadedFileName = null;
            }
            else
            {
                slideMainView.texture = texture;
                slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
                _mainLoadedSource = source;
                _mainLoadedFileName = fileName;
            }

        }
    }
}
