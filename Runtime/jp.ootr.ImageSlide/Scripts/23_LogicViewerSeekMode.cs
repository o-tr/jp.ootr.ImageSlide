using jp.ootr.ImageSlide.Viewer;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace jp.ootr.ImageSlide
{
    public class LogicViewerSeekMode : LogicQueue
    {
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

        protected override void DoSyncAll()
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.SyncAll);
            var sourceDic = new DataList();
            var optionDic = new DataList();
            for (var i = 0; i < Sources.Length; i++)
            {
                sourceDic.Add(Sources[i]);
                optionDic.Add(Options[i]);
            }

            dic.SetValue("sources", sourceDic);
            dic.SetValue("options", optionDic);
            dic.SetValue("index", currentIndex);
            dic.SetValue("seekMode", (int)seekMode);

            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                ConsoleError($"failed to serialize sync all json: {json}");
                ProcessQueue();
                return;
            }

            AddSyncQueue(json.String);
            ProcessQueue();
        }
    }
}
