using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UINextSlide : UIThumbnails
    {
        private readonly string _nextTextureLoadChannel = "jp.ootr.ImageSlide.UINextSlide.NextTextureLoader";

        [SerializeField] private RawImage slideNextView;
        [SerializeField] private AspectRatioFitter slideNextViewFitter;
        private string _nextLoadedFileName;
        private string _nextLoadedSource;

        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();
            SetNextTexture(currentIndex);
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            SetNextTexture(index);
        }

        private void SetNextTexture(int index)
        {
            var nextIndex = index + 1;
            if (nextIndex >= slideCount)
            {
                slideNextView.texture = blankTexture;
                _nextLoadedSource = null;
                _nextLoadedFileName = null;
                return;
            }

            var currentSource = FlatSources[nextIndex];
            var currentFileName = FlatFileNames[nextIndex];

            if (_nextLoadedSource != null && _nextLoadedFileName != null)
            {
                ConsoleInfo($"unload main: {_nextLoadedSource} / {_nextLoadedFileName}");
                controller.CcReleaseTexture(_nextLoadedSource, _nextLoadedFileName);
                _nextLoadedSource = null;
                _nextLoadedFileName = null;
            }

            _nextLoadedSource = currentSource;
            _nextLoadedFileName = currentFileName;

            controller.LoadFile(this, _nextLoadedSource, _nextLoadedFileName, 50, _nextTextureLoadChannel);
        }

        public override void OnFileLoadSuccess(string sourceUrl, string fileUrl, string channel)
        {
            base.OnFileLoadSuccess(sourceUrl, fileUrl, channel);
            if (fileUrl == null) return;
            if (_nextTextureLoadChannel != channel) return;
            ConsoleDebug($"next slide image loaded: {fileUrl}");
            if (_nextLoadedSource != sourceUrl || _nextLoadedFileName != fileUrl)
            {
                ConsoleDebug($"next slide image not match: {sourceUrl} / {fileUrl} != {_nextLoadedSource} / {_nextLoadedFileName}");
                return;
            }
            var texture = controller.CcGetTexture(sourceUrl, fileUrl);
            if (texture == null) return;
            slideNextView.texture = texture;
            slideNextViewFitter.aspectRatio = (float)texture.width / texture.height;
        }
    }
}
