using JetBrains.Annotations;
using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UIThumbnails : UISlide {
        private const int SlideListViewBaseThumbnailWidth = 375;
        private const int SlideListViewBaseGap = 16;
        private const int SlideListViewBasePadding = 16;
        [SerializeField] private ScrollRect slideListView;
        [SerializeField] private Transform slideListViewRoot;
        [SerializeField] private RectTransform slideListViewRootRectTransform;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;
        [SerializeField] private RectTransform slideListViewRectTransform;
        [NotNull] private AspectRatioFitter[] _slideListFitters = new AspectRatioFitter[0];
        [NotNull] [ItemCanBeNull] private string[] _slideListLoadedFileNames = new string[0];

        [NotNull] [ItemCanBeNull] private string[] _slideListLoadedSources = new string[0];
        [NotNull] private TextMeshProUGUI[] _slideListTexts = new TextMeshProUGUI[0];
        [NotNull] private RawImage[] _slideListThumbnails = new RawImage[0];
        [NotNull] private Toggle[] _slideListToggles = new Toggle[0];
        

        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();
            BuildSlideList();
        }

        public void OnSlideListClicked()
        {
            if (!_slideListToggles.HasChecked(out var index)) return;
            SeekTo(index);
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            
            var offset =
                (index * (SlideListViewBaseThumbnailWidth + SlideListViewBaseGap) - SlideListViewBaseGap +
                 SlideListViewBasePadding) / (slideListViewRootRectTransform.rect.width -
                                              slideListViewRectTransform.rect.width);
            slideListView.horizontalNormalizedPosition = Mathf.Max(Mathf.Min(offset, 1), 0);
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
                for (var i = slideCount; i < currentLength; i++) DestroyImmediate(_slideListToggles[i].gameObject);

                _slideListToggles = _slideListToggles.Resize(slideCount);
                _slideListThumbnails = _slideListThumbnails.Resize(slideCount);
                _slideListFitters = _slideListFitters.Resize(slideCount);
                _slideListTexts = _slideListTexts.Resize(slideCount);
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

        }

    }
}
