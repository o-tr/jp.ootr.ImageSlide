using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UISlide : UISeekMode
    {
        [SerializeField] internal ImageSlide imageSlide;

        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;



        [SerializeField] private Texture2D blankTexture;

        protected readonly int _animatorFollowMaster = Animator.StringToHash("FollowMaster");

        protected bool _followMaster = true;
        protected int _localIndex;
        private string _mainLoadedFileName;

        private string _mainLoadedSource;
        protected int _masterIndex;

        protected int _maxIndex;


        

        public void SeekToNext()
        {
            if (imageSlide.slideCount <= _localIndex + 1 || SeekMode == SeekMode.DisallowAll) return;
            if ((SeekMode == SeekMode.AllowViewedOnly || SeekMode == SeekMode.AllowPreviousOnly) &&
                _localIndex + 1 > _maxIndex) return;
            _followMaster = false;
            animator.SetBool(_animatorFollowMaster, false);
            SeekTo(++_localIndex);
        }

        public void SeekToPrevious()
        {
            if (_localIndex <= 0 || SeekMode == SeekMode.DisallowAll) return;
            _followMaster = false;
            animator.SetBool(_animatorFollowMaster, false);
            SeekTo(--_localIndex);
        }

        public void FollowMaster()
        {
            _followMaster = true;
            animator.SetBool(_animatorFollowMaster, true);
            _localIndex = _masterIndex;
            SeekTo(_masterIndex);
        }

        protected void SeekTo(int index)
        {
            LocalIndexUpdated(index);
            SetTexture(index);
        }

        private void SetTexture(int index)
        {
            if (!imageSlide.FileNames.GetByIndex(index, out var sourceIndex, out var fileIndex)) return;
            var source = imageSlide.GetSources()[sourceIndex];
            var fileName = imageSlide.FileNames[sourceIndex][fileIndex];
            var controller = imageSlide.GetController();
            ConsoleInfo($"load main: {source} / {fileName}");
            var texture = controller.CcGetTexture(source, fileName);

            if (_mainLoadedSource != null && _mainLoadedFileName != null)
            {
                ConsoleInfo($"unload main: {_mainLoadedSource} / {_mainLoadedFileName}");
                controller.CcReleaseTexture(_mainLoadedSource, _mainLoadedFileName);
            }

            if (texture == null)
            {
                slideMainView.texture = blankTexture;
                _mainLoadedSource = null;
                _mainLoadedFileName = null;
            }
            else
            {
                slideMainView.texture = texture;
                slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
                _mainLoadedSource = source;
                _mainLoadedFileName = fileName;
            }
        }
    }
}
