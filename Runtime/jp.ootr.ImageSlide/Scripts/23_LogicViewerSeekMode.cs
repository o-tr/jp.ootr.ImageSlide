using jp.ootr.ImageSlide.Viewer;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class LogicViewerSeekMode : LogicQueue {
        [SerializeField] internal SeekMode seekMode;
        [SerializeField] private ToggleGroup seekModeToggleGroup;
        [SerializeField] private Toggle allowAllToggle;
        [SerializeField] private Toggle allowPreviousOnlyToggle;
        [SerializeField] private Toggle allowViewedOnlyToggle;
        [SerializeField] private Toggle disallowAllToggle;

        public override void InitController()
        {
            base.InitController();
            UpdateToggleGroup();
            SeekModeChanged(seekMode);
        }
        
        private void UpdateToggleGroup()
        {
            allowAllToggle.isOn = seekMode == SeekMode.AllowAll;
            allowPreviousOnlyToggle.isOn = seekMode == SeekMode.AllowPreviousOnly;
            allowViewedOnlyToggle.isOn = seekMode == SeekMode.AllowViewedOnly;
            disallowAllToggle.isOn = seekMode == SeekMode.DisallowAll;
        }

        public void UpdateSeekMode()
        {
            var value = seekModeToggleGroup.GetFirstActiveToggle();
            if (value == null) return;
            var mode = value.name;
            switch (mode)
            {
                case "AllowAll":
                    SeekModeChanged(SeekMode.AllowAll);
                    break;
                case "AllowPreviousOnly":
                    SeekModeChanged(SeekMode.AllowPreviousOnly);
                    break;
                case "AllowViewedOnly":
                    SeekModeChanged(SeekMode.AllowViewedOnly);
                    break;
                case "DisallowAll":
                    SeekModeChanged(SeekMode.DisallowAll);
                    break;
                default:
                    ConsoleError($"Unknown seek mode: {mode}");
                    break;
            }
        }
    }
}
