using jp.ootr.ImageSlide.Viewer;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class LogicViewerSeekMode : LogicQueue {
        [SerializeField] internal SeekMode seekMode;
        [SerializeField] private ToggleGroup seekModeToggleGroup;
        [SerializeField] private Toggle allowAllToggle;
        [SerializeField] private Toggle allowPreviousOnlyToggle;
        [SerializeField] private Toggle allowViewedOnlyToggle;
        [SerializeField] private Toggle disallowAllToggle;

        private bool _isSeekModeChangedByScript;
        
        public override void InitController()
        {
            base.InitController();
            UpdateToggleGroup();
            SeekModeChanged(seekMode);
        }
        
        private void UpdateToggleGroup()
        {
            _isSeekModeChangedByScript = true;
            allowAllToggle.isOn = seekMode == SeekMode.AllowAll;
            allowPreviousOnlyToggle.isOn = seekMode == SeekMode.AllowPreviousOnly;
            allowViewedOnlyToggle.isOn = seekMode == SeekMode.AllowViewedOnly;
            disallowAllToggle.isOn = seekMode == SeekMode.DisallowAll;
            _isSeekModeChangedByScript = false;
        }

        public void UpdateSeekMode()
        {
            if (_isSeekModeChangedByScript) return;
            var value = seekModeToggleGroup.GetFirstActiveToggle();
            if (!Utilities.IsValid(value)) return;
            var mode = value.name;
            switch (mode)
            {
                case "AllowAll":
                    SyncSeekMode(SeekMode.AllowAll);
                    break;
                case "AllowPreviousOnly":
                    SyncSeekMode(SeekMode.AllowPreviousOnly);
                    break;
                case "AllowViewedOnly":
                    SyncSeekMode(SeekMode.AllowViewedOnly);
                    break;
                case "DisallowAll":
                    SyncSeekMode(SeekMode.DisallowAll);
                    break;
                default:
                    ConsoleError($"Unknown seek mode: {mode}");
                    break;
            }
        }

        protected override void SeekModeChanged(SeekMode mode)
        {
            base.SeekModeChanged(mode);
            seekMode = mode;
            UpdateToggleGroup();
        }
    }
}
