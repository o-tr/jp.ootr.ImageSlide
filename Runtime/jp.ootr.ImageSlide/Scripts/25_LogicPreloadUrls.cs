using JetBrains.Annotations;
using jp.ootr.ImageDeviceController;
using UnityEngine;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class LogicPreloadUrls : LogicResetTransform
    {
        [ItemCanBeNull][SerializeField] internal string[] definedSources = new string[0];
        [SerializeField] internal SourceType[] definedSourceTypes = new SourceType[0];
        [SerializeField] internal float[] definedSourceOffsets = new float[0];
        [SerializeField] internal float[] definedSourceIntervals = new float[0];
        [ItemCanBeNull][SerializeField] internal VRCUrl[] definedSourceUrls = new VRCUrl[0];

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            if (!Networking.IsOwner(gameObject)) return;
            if (!player.isLocal) return;

            foreach (var url in definedSourceUrls)
            {
                if (url == null) continue;
                controller.UsAddUrl(url);
            }

            for (var i = 0; i < definedSources.Length; i++)
            {
                if (definedSources[i] == null) continue;
                AddSourceQueue(definedSources[i],
                    UrlUtil.BuildSourceOptions(definedSourceTypes[i], definedSourceOffsets[i],
                        definedSourceIntervals[i]));
            }
        }
    }
}
