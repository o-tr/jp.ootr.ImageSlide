using JetBrains.Annotations;
using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace jp.ootr.ImageSlide
{
    public class UISlide : UIDeviceList
    {
        private readonly string _mainTextureLoadChannel = "jp.ootr.ImageSlide.UISlide.MainTextureLoader";

        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;


        [SerializeField] private TextMeshProUGUI slideMainViewNote;


        [SerializeField] private TextMeshProUGUI slideCountText;


        [SerializeField] private Texture2D splashScreen;
        [SerializeField] protected Texture2D blankTexture;

        private string _mainLoadedFileName;
        private string _mainLoadedSource;


        public void SeekToNext()
        {
            if (slideCount <= currentIndex + 1) return;
            SeekTo(currentIndex + 1);
        }

        public void SeekToPrevious()
        {
            if (currentIndex <= 0) return;
            SeekTo(currentIndex - 1);
        }

        public void SeekToStart()
        {
            if (currentIndex == 0) return;
            SeekTo(0);
        }

        public void SeekToEnd()
        {
            if (currentIndex == slideCount - 1) return;
            SeekTo(slideCount - 1);
        }

        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();

            SetTexture(currentIndex);
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            SetTexture(index);
        }

        private void SetTexture(int index)
        {
            slideCountText.text = $"{index + 1} / {slideCount}";
            ConsoleDebug($"slide index updated: {index} / {slideCount}");
            if (index < 0 || index >= slideCount)
            {
                animator.SetBool(AnimatorSplash, true);
                ConsoleError($"invalid index: {index}");
                return;
            }

            var currentSource = FlatSources[index];
            var currentFileName = FlatFileNames[index];
            ConsoleInfo($"load texture: {currentSource} / {currentFileName}");

            CastToScreens(currentSource, currentFileName);

            if (_mainLoadedSource != null && _mainLoadedFileName != null)
            {
                ConsoleInfo($"unload main: {_mainLoadedSource} / {_mainLoadedFileName}");
                controller.CcReleaseTexture(_mainLoadedSource, _mainLoadedFileName);
                _mainLoadedSource = null;
                _mainLoadedFileName = null;
            }

            _mainLoadedSource = currentSource;
            _mainLoadedFileName = currentFileName;

            animator.SetBool(AnimatorSplash, false);
            animator.SetBool(AnimatorIsLoading, true);
            controller.LoadFile(this, _mainLoadedSource, _mainLoadedFileName, 100, _mainTextureLoadChannel);
        }

        public override void OnFileLoadSuccess(string sourceUrl, string fileUrl, string channel)
        {
            base.OnFileLoadSuccess(sourceUrl, fileUrl, channel);
            if (fileUrl == null) return;
            if (channel != _mainTextureLoadChannel) return;
            ConsoleDebug($"slide image loaded: {fileUrl}");
            if (_mainLoadedFileName != fileUrl || _mainLoadedSource != sourceUrl)
            {
                ConsoleDebug($"main texture not match: {_mainLoadedFileName} / {_mainLoadedSource} != {fileUrl} / {sourceUrl}");
                return;
            }
            var texture = controller.CcGetTexture(sourceUrl, fileUrl);
            if (texture == null) return;
            animator.SetBool(AnimatorIsLoading, false);
            
            SetNote(sourceUrl, fileUrl);
            if (texture != slideMainView.texture)
            {
                slideMainView.texture = texture;
                slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
            }
        }

        private void CastToScreens(string source, string fileName)
        {
            if (!Networking.IsOwner(gameObject)) return;
            foreach (var device in devices)
            {
                if (device == null || !device.IsCastableDevice() ||
                    !deviceSelectedUuids.Has(device.deviceUuid)) continue;
                device.LoadImage(source, fileName);
            }
        }

        private void SetNote(string currentSource, string currentFileName)
        {
            var metadata = controller.CcGetMetadata(currentSource, currentFileName);
            if (metadata == null) return;
            var extensions = metadata.GetExtensions();
            if (extensions.TryGetValue("note", TokenType.String, out var note))
                slideMainViewNote.text = note.ToString();
            else
                slideMainViewNote.text = "";
        }

        public DeviceController GetController()
        {
            return controller;
        }
    }
}
