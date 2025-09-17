using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UIDeviceList : EventSourceList
    {
        [SerializeField] internal Transform settingsTransform;
        [SerializeField] private TextMeshProUGUI settingsTitleText;
        [SerializeField] internal Transform rootDeviceTransform;
        [SerializeField] internal TextMeshProUGUI rootDeviceNameText;
        [SerializeField] internal RawImage rootDeviceIcon;
        [SerializeField] internal Toggle rootDeviceToggle;
        [SerializeField] internal string[] deviceSelectedUuids;
        [SerializeField] internal bool isDeviceListLocked;
        private Toggle[] _deviceToggles = new Toggle[0];

        public override void InitController()
        {
            base.InitController();
            if (isDeviceListLocked) settingsTitleText.text = $"{settingsTitleText.text} (Locked)";
            _deviceToggles = new Toggle[rootDeviceTransform.childCount];
            var index = 0;
            foreach (Transform trans in rootDeviceTransform)
            {
                if (trans.name.StartsWith("_")) continue;
                var toggle = trans.GetComponent<Toggle>();
                if (toggle == null) continue;
                _deviceToggles[index++] = toggle;
                if (isDeviceListLocked) toggle.interactable = false;
            }

            _deviceToggles = _deviceToggles.Resize(index);
            OnDeviceListUpdate();
        }

        public void OnDeviceListUpdate()
        {
            if (isDeviceListLocked) return;
            deviceSelectedUuids = new string[_deviceToggles.Length];
            var index = 0;
            foreach (var toggle in _deviceToggles)
            {
                if (!toggle.isOn) continue;
                deviceSelectedUuids[index++] = toggle.name;
            }

            deviceSelectedUuids = deviceSelectedUuids.Resize(index);
        }
    }
}
