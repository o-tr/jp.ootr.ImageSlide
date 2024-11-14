using jp.ootr.common;
using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public enum SeekMode
    {
        AllowAll,
        AllowPreviousOnly,
        AllowViewedOnly,
        DisallowAll
    }
    public class UISeekMode : BaseClass
    {
        protected SeekMode seekMode;
        [SerializeField] private RectTransform presentationTransform;
        [SerializeField] private GameObject mainView;
        [SerializeField] private GameObject slideList;

        public override void InitImageSlide()
        {
            base.InitImageSlide();
            UpdatePresentationView();
        }

        public override void SeekModeChanged(SeekMode mode)
        {
            base.SeekModeChanged(seekMode);
            seekMode = mode;
            UpdatePresentationView();
        }

        private void UpdatePresentationView()
        {
            slideList.SetActive(seekMode != SeekMode.DisallowAll);
            presentationTransform.ToFillChildrenVertical(4);
        }
    }
}
