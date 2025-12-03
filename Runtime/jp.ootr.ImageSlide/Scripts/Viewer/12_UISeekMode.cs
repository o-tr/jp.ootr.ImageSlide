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
        [SerializeField] private RectTransform presentationTransform;
        [SerializeField] private GameObject mainView;
        [SerializeField] private GameObject slideList;
        protected SeekMode SeekMode;

        public override void InitImageSlide()
        {
            base.InitImageSlide();
            UpdatePresentationView();
        }

        public override void SeekModeChanged(SeekMode mode)
        {
            base.SeekModeChanged(mode);
            SeekMode = mode;
            UpdatePresentationView();
        }

        private void UpdatePresentationView()
        {
            slideList.SetActive(SeekMode != SeekMode.DisallowAll);
            presentationTransform.ToFillChildrenVertical(4);
        }
    }
}
