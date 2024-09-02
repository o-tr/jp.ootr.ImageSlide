using jp.ootr.ImageDeviceController.CommonDevice;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UIAnimationHandler : CommonDevice
    {
        [SerializeField] private ToggleGroup uITabGroup;
        private readonly int _animatorOverlay = Animator.StringToHash("Overlay");
        private readonly int _animatorTab = Animator.StringToHash("Tab");

        public void OnTabSelected()
        {
            var active = uITabGroup.GetFirstActiveToggle();
            switch (active.name)
            {
                case "Presentation":
                    animator.SetInteger(_animatorTab, 0);
                    break;
                case "Slides":
                    animator.SetInteger(_animatorTab, 1);
                    break;
                case "Settings":
                    animator.SetInteger(_animatorTab, 2);
                    break;
            }
        }

        public void OnAddVideoClick()
        {
            animator.SetInteger(_animatorOverlay, 1);
        }

        public void OnCloseOverlay()
        {
            animator.SetInteger(_animatorOverlay, 0);
        }
    }
}