using System;
using jp.ootr.ImageDeviceController.CommonDevice;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UIDeviceList : LogicQueue {
        [SerializeField] public TextMeshProUGUI rootDeviceNameText;
        [SerializeField] public RawImage rootDeviceIcon;
        [SerializeField] public Toggle rootDeviceToggle;
        [SerializeField] public string[] deviceSelectedUuids;
        private Toggle[] _deviceToggles;
    }
}