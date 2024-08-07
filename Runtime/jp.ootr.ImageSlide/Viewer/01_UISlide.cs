using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UISlide : BaseClass {
        [SerializeField] protected ImageSlide imageSlide;
        
        [SerializeField] protected Animator animator;
        
        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;
        
        [SerializeField] private Transform slideListViewRoot;
        [SerializeField] private GameObject slideListViewBase;
        [SerializeField] private RawImage slideListViewBaseThumbnail;
        [SerializeField] private AspectRatioFitter slideListViewBaseFitter;
        [SerializeField] private TextMeshProUGUI slideListViewBaseText;
        private Toggle[] _slideListToggles;

        [SerializeField] private ScrollRect slideListView;
        
        [SerializeField] private Texture2D blankTexture;
        
        private readonly int _slideListViewBaseThumbnailWidth = 375;
        private readonly int _slideListViewBaseGap = 16;
        private readonly int _slideListViewBasePadding = 16;
        
        private bool _followMaster = true;
        private int _localIndex = 0;
        private int _masterIndex = 0;
        
        private readonly int _animatorFollowMaster = Animator.StringToHash("FollowMaster");
        
        public override void UrlsUpdated()
        {
            base.UrlsUpdated();
            slideListViewRoot.ClearChildren();
            BuildSlideList();
        }
        
        private void BuildSlideList()
        {
            _slideListToggles = new Toggle[imageSlide.slideCount];
            var index = 0;
            for (int i = 0; i < imageSlide.FileNames.Length; i++)
            {
                var fileList = imageSlide.FileNames[i];
                var textures = imageSlide.Textures[i];
                for (int j = 0; j < fileList.Length; j++)
                {
                    var fileName = fileList[j];
                    var texture = textures[j];
                    slideListViewBaseThumbnail.texture = texture;
                    slideListViewBaseFitter.aspectRatio = (float)texture.width / texture.height;
                    slideListViewBaseText.text = (index+1).ToString();
                    var obj = Instantiate(slideListViewBase, slideListViewRoot);
                    obj.name = fileName;
                    obj.SetActive(true);
                    obj.transform.SetSiblingIndex(index);
                    _slideListToggles[index] = obj.transform.Find("__IDENTIFIER").GetComponent<Toggle>();
                    index++;
                }
            }
            slideListViewRoot.ToListChildrenHorizontal(16,16,true);
            SetTexture(imageSlide.currentIndex);
        }

        public override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            _masterIndex = index;
            if (_followMaster)
            {
                _localIndex = index;
                SeekTo(index);
            }
        }
        
        public void OnSlideListClicked()
        {
            if (!_slideListToggles.HasChecked(out var index)) return;
            _followMaster = false;
            animator.SetBool(_animatorFollowMaster, false);
            _localIndex = index;
            SeekTo(index);
        }
        
        public void SeekToNext()
        {
            if (imageSlide.slideCount <= _localIndex + 1) return;
            _followMaster = false;
            animator.SetBool(_animatorFollowMaster, false);
            SeekTo(++_localIndex);
        }
        
        public void SeekToPrevious()
        {
            if (imageSlide.currentIndex <= 0) return;
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
            slideListView.horizontalNormalizedPosition = Mathf.Max(Mathf.Min(offset, 1),0);
            SetTexture(index);
        }

        private void SetTexture(int index)
        {
            var texture = imageSlide.Textures.GetByIndex(index);
            if (texture == null)
            {
                slideMainView.texture = blankTexture;
                return;
            }
            slideMainView.texture = texture;
            slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
        }
    }
}