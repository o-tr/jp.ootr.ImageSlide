using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;

namespace jp.ootr.ImageSlide
{
    public class UISlide : UIDeviceList
    {
        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;
        
        [SerializeField] private Transform slideListViewRoot;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private Slider slideListViewBaseSlider;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;

        [SerializeField] private ScrollRect slideListView;
        
        private readonly int _slideListViewBaseThumbnailWidth = 375;
        private readonly int _slideListViewBaseGap = 16;
        private readonly int _slideListViewBasePadding = 16;

        public void SeekToNext()
        {
            if (SlideCount <= CurrentIndex + 1) return;
            SeekTo(CurrentIndex + 1);
        }
        
        public void SeekToPrevious()
        {
            if (CurrentIndex <= 0) return;
            SeekTo(CurrentIndex - 1);
        }
        
        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();
            slideListViewRoot.ClearChildren();
            BuildSlideList();
        }
        
        private void BuildSlideList()
        {
            var index = 0;
            for (int i = 0; i < FileNames.Length; i++)
            {
                var fileList = FileNames[i];
                var textures = Textures[i];
                for (int j = 0; j < fileList.Length; j++)
                {
                    var fileName = fileList[j];
                    var texture = textures[j];
                    slideListViewBaseThumbnail.texture = texture;
                    slideListViewBaseSlider.value = index;
                    slideListViewBaseFitter.aspectRatio = (float)texture.width / texture.height;
                    slideListViewBaseText.text = (index+1).ToString();
                    var obj = Instantiate(slideListViewBase, slideListViewRoot);
                    obj.name = fileName;
                    obj.SetActive(true);
                    obj.transform.SetSiblingIndex(index);
                    index++;
                }
            }
            slideListViewRoot.ToListChildrenHorizontal(16,16,true);
            if (SlideCount > 0)
            {
                SetTexture(CurrentIndex);
            }
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            var offset =
                (index * (_slideListViewBaseThumbnailWidth + _slideListViewBaseGap) - _slideListViewBaseGap +
                 _slideListViewBasePadding) / (slideListViewRoot.GetComponent<RectTransform>().rect.width -
                                               slideListView.GetComponent<RectTransform>().rect.width);
            slideListView.horizontalNormalizedPosition = Mathf.Max(Mathf.Min(offset, 1),0);
            SetTexture(index);
        }

        private void SetTexture(int index)
        {
            var texture = Textures.GetByIndex(index, out var sourceIndex, out var fileIndex);
            slideMainView.texture = texture;
            slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
            var source = FileNames[sourceIndex][fileIndex];
            foreach (var device in devices)
            {
                if (!device.IsCastableDevice()||!deviceSelectedUuids.Has(device.deviceUuid)) continue;
                Debug.Log($"LoadImage {Sources[sourceIndex]}, {source}");
                device.LoadImage(Sources[sourceIndex], source);
            }
        }
    }
}