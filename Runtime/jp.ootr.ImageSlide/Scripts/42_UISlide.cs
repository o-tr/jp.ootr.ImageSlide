﻿using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;

namespace jp.ootr.ImageSlide
{
    public class UISlide : UIDeviceList
    {
        private const int SlideListViewBaseThumbnailWidth = 375;
        private const int SlideListViewBaseGap = 16;
        private const int SlideListViewBasePadding = 16;
        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;
        [SerializeField] private TextMeshProUGUI slideMainViewNote;

        [SerializeField] private Transform slideListViewRoot;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;

        [SerializeField] private ScrollRect slideListView;

        [SerializeField] private Texture2D splashScreen;

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
                    slideListViewBaseThumbnail.texture = texture;
                    slideListViewBaseFitter.aspectRatio = (float)texture.width / texture.height;
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
            var texture = Textures.GetByIndex(index, out var sourceIndex, out var fileIndex);
            animator.SetBool(_animatorSplash, texture == null);
            if (texture == null) return;
            slideMainView.texture = texture;
            slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
            var source = FileNames[sourceIndex][fileIndex];
            var metadata = controller.CcGetMetadata(Sources[sourceIndex], source);
            if (metadata.GetExtensions().TryGetValue("note", TokenType.String, out var note))
                slideMainViewNote.text = note.ToString();
            else
                slideMainViewNote.text = "";
            foreach (var device in devices)
            {
                if (device == null || !device.IsCastableDevice() ||
                    !deviceSelectedUuids.Has(device.deviceUuid)) continue;
                device.LoadImage(Sources[sourceIndex], source);
            }
        }

        private void SetNote(int index)
        {
        }
    }
}
