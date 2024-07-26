using System;
using jp.ootr.ImageDeviceController;
using jp.ootr.ImageDeviceController.CommonDevice;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon.Common.Enums;

namespace jp.ootr.ImageSlide
{
    public class UIDeviceList : UIAnimationHandler {
        [SerializeField] private TextMeshProUGUI _rootDeviceNameText;
        [SerializeField] private RawImage _rootDeviceIcon;
        
        private Toggle[] _deviceToggles;
        
        public override void InitController()
        {
            base.InitController();
            SendCustomEventDelayedFrames(nameof(UpdateDeviceList), 1, EventTiming.LateUpdate);
        }
        
        public virtual void UpdateDeviceList()
        {
            var tmpArray = new Toggle[devices.Length];
            var tmpIndex = 0;
            for (var i = 0; i < devices.Length; i++)
            {
                var device = devices[i];
                if (device == null || device.GetDeviceUuid() == deviceUuid) continue;
                if (!device.IsCastableDevice()) continue;
                tmpArray[tmpIndex++] = CreateDeviceItem(device);
            }

            _deviceToggles = new Toggle[tmpIndex];
            Array.Copy(tmpArray, _deviceToggles, tmpIndex);
        }

        private Toggle CreateDeviceItem(CommonDevice device)
        {
            _rootDeviceNameText.text = device.GetName();
            _rootDeviceIcon.texture = device.deviceIcon;
            var obj = Instantiate(_rootDeviceNameText.transform.parent.gameObject, _rootDeviceNameText.transform.parent.parent);
            obj.SetActive(true);
            return obj.GetComponent<Toggle>();;
        }
    }
}