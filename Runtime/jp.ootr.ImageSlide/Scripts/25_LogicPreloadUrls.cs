using jp.ootr.ImageDeviceController;
using UnityEngine;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class LogicPreloadUrls : LogicResetTransform
    {
        [SerializeField] internal string[] definedSources = new string[0];
        [SerializeField] internal URLType[] definedSourceTypes = new URLType[0];
        [SerializeField] internal float[] definedSourceOffsets = new float[0];
        [SerializeField] internal float[] definedSourceIntervals = new float[0];
        [SerializeField] internal VRCUrl[] definedSourceUrls = new VRCUrl[0];

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            if (!Networking.IsOwner(gameObject)) return;
            if (!player.isLocal) return;

            foreach (var url in definedSourceUrls) controller.UsAddUrl(url);
            for (var i = 0; i < definedSources.Length; i++)
                AddSourceQueue(definedSources[i],
                    UrlUtil.BuildSourceOptions(definedSourceTypes[i], definedSourceOffsets[i],
                        definedSourceIntervals[i]));
        }
    }
}
