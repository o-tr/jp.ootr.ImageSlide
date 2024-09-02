using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UIDeviceList : EventSourceList
    {
        [SerializeField] public Transform settingsTransform;
        [SerializeField] public Transform rootDeviceTransform;
        [SerializeField] public TextMeshProUGUI rootDeviceNameText;
        [SerializeField] public RawImage rootDeviceIcon;
        [SerializeField] public Toggle rootDeviceToggle;
        [SerializeField] public string[] deviceSelectedUuids;
        private Toggle[] _deviceToggles = new Toggle[0];

        public override void InitController()
        {
            base.InitController();
            _deviceToggles = new Toggle[rootDeviceTransform.childCount];
            var index = 0;
            foreach (Transform trans in rootDeviceTransform)
            {
                if (trans.name.StartsWith("_")) continue;
                var toggle = trans.GetComponent<Toggle>();
                if (toggle == null) continue;
                _deviceToggles[index++] = toggle;
            }

            _deviceToggles = _deviceToggles.Resize(index);
        }

        public void OnDeviceListUpdate()
        {
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