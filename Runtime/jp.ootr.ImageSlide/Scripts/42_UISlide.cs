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
            for (var i = 0; i < FileNames.Length; i++)
            {
                var fileList = FileNames[i];
                var textures = Textures[i];
                for (var j = 0; j < fileList.Length; j++)
                {
                    var fileName = fileList[j];
                    var texture = textures[j];
                    if (texture != null)
                    {
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

            var texture = Textures.GetByIndex(index, out var sourceIndex, out var fileIndex);
            animator.SetBool(_animatorSplash, texture == null || slideCount == 0);
            if (texture != null)
            {
                slideMainView.texture = texture;
                slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
                var fileName = FileNames[sourceIndex][fileIndex];
                var metadata = controller.CcGetMetadata(Sources[sourceIndex], fileName);
                SetNote(metadata);
                CastToScreens(Sources[sourceIndex], fileName);
            }

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
            var nextTexture = Textures.GetByIndex(nextIndex, out var void1, out var void2);
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
    }
}
