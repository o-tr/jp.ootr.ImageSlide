using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using UnityEngine;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class EventSourceList : UISourceList
    {
        public void OnSourceEndEdit()
        {
            GetUrl(out var url, out var type, out var options);
            if (url.ToString().IsNullOrEmpty()) return;
            if (!url.ToString().IsValidUrl(out var error))
            {
                OnFilesLoadFailed(error);
                return;
            }

            AddUrl(url, type, options);
            ResetInputs();
        }

        public void OnVideoSourceEndEdit()
        {
            var source = sourceVideoUrlInput.GetUrl();
            if (source.ToString().IsNullOrEmpty()) return;
            if (!source.ToString().IsValidUrl(out var error))
            {
                OnFilesLoadFailed(error);
                return;
            }

            sourceVideoUrlInput.SetUrl(VRCUrl.Empty);
            var options = UrlUtil.BuildSourceOptions(URLType.Video, sourceVideoOffsetSlider.value,
                sourceVideoIntervalSlider.value);
            AddUrl(source, URLType.Video, options);
            ResetInputs();
            OnCloseOverlay();
        }

        public void OnVideoOffsetSliderChange()
        {
            var value = Mathf.Round(sourceVideoOffsetSlider.value * 10) / 10;
            sourceVideoOffsetInput.text = value.ToString();
            sourceVideoOffsetSlider.value = value;
        }

        public void OnVideoOffsetEndEdit()
        {
            sourceVideoOffsetSlider.value = float.Parse(sourceVideoOffsetInput.text);
        }

        public void OnVideoIntervalSliderChange()
        {
            var value = Mathf.Round(sourceVideoIntervalSlider.value * 10) / 10;
            sourceVideoIntervalInput.text = value.ToString();
            sourceVideoIntervalSlider.value = value;
        }

        public void OnVideoIntervalEndEdit()
        {
            sourceVideoIntervalSlider.value = float.Parse(sourceVideoIntervalInput.text);
        }

        public void OnSourceDelete()
        {
            if (!SourceToggles.HasChecked(out var index)) return;
            RemoveSourceQueue(SourceToggles[index].transform.parent.name);
        }
    }
}
