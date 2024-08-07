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
        private Toggle[] deviceToggles = new Toggle[0];

        public override void InitController()
        {
            base.InitController();
            deviceToggles = new Toggle[rootDeviceTransform.childCount];
            var index = 0;
            foreach (Transform trans in rootDeviceTransform)
            {
                if (trans.name.StartsWith("_")) continue;
                var toggle = trans.GetComponent<Toggle>();
                if (toggle == null) continue;
                deviceToggles[index++] = toggle;
            }

            deviceToggles = deviceToggles.Resize(index);
            Debug.Log($"device toggle: {deviceToggles.Length}, {rootDeviceTransform.childCount}");
        }

        public void OnDeviceListUpdate()
        {
            deviceSelectedUuids = new string[deviceToggles.Length];
            var index = 0;
            foreach (var toggle in deviceToggles)
            {
                if (!toggle.isOn) continue;
                deviceSelectedUuids[index++] = toggle.name;
            }

            Debug.Log($"device: {deviceSelectedUuids.Length}");
            deviceSelectedUuids = deviceSelectedUuids.Resize(index);
        }
    }
}