using jp.ootr.common;
using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UISeekDisable : BaseClass
    {
        [SerializeField] public bool seekDisabled;
        [SerializeField] private RectTransform presentationTransform;
        [SerializeField] private GameObject mainView;
        [SerializeField] private GameObject slideList;

        public void SetSeekDisabled(bool disabled)
        {
            seekDisabled = disabled;
            slideList.SetActive(!seekDisabled);
            presentationTransform.ToFillChildrenVertical(4);
        }
    }
}
