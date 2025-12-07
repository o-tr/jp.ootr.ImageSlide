using jp.ootr.ImageDeviceController;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UISlide : UISeekMode
    {
        [SerializeField] internal ImageSlide imageSlide;

        [SerializeField] private RawImage slideMainView;
        [SerializeField] private AspectRatioFitter slideMainViewFitter;

        private readonly string _mainTextureLoadChannel = "jp.ootr.ImageSlide.Viewer.UISlide.MainTextureLoader";

        [SerializeField] private Texture2D blankTexture;


        protected bool _followMaster = true;
        protected int _localIndex;
        private string _mainLoadedFileName;

        private string _mainLoadedSource;
        protected int _masterIndex;

        protected int _maxIndex;

        // フォールバックで即時適用したかを記録（CacheControllerの使用カウント二重加算防止）
        private bool _mainAppliedFromCache;

        protected override void Start()
        {
            base.Start();
            controller = imageSlide.GetController();
        }

        public override void UrlsUpdated()
        {
            base.UrlsUpdated();
            // URLが更新された時に、現在のインデックスの画像を読み込む
            if (imageSlide != null && imageSlide.slideCount > 0)
            {
                if (_followMaster)
                {
                    // マスターに追従している場合、マスターのインデックスを使用
                    SeekTo(_masterIndex);
                }
                else
                {
                    // ローカルで操作している場合、ローカルインデックスを使用
                    SeekTo(_localIndex);
                }
            }
        }

        public override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            _masterIndex = index;
            // マスターに追従している場合、マスターのインデックスに合わせて画像を読み込む
            if (_followMaster)
            {
                _localIndex = index;
                SeekTo(index);
            }
        }

        public void SeekToNext()
        {
            if (imageSlide.slideCount <= _localIndex + 1 || SeekMode == SeekMode.DisallowAll) return;
            if ((SeekMode == SeekMode.AllowViewedOnly || SeekMode == SeekMode.AllowPreviousOnly) &&
                _localIndex + 1 > _maxIndex) return;
            _followMaster = false;
            animator.SetBool(AnimatorFollowMaster, false);
            SeekTo(++_localIndex);
        }

        public void SeekToPrevious()
        {
            if (_localIndex <= 0 || SeekMode == SeekMode.DisallowAll) return;
            _followMaster = false;
            animator.SetBool(AnimatorFollowMaster, false);
            SeekTo(--_localIndex);
        }

        public void FollowMaster()
        {
            _followMaster = true;
            animator.SetBool(AnimatorFollowMaster, true);
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
            ConsoleInfo($"load main: {source} / {fileName}");
            _mainAppliedFromCache = false;
            if (_mainLoadedSource != null && _mainLoadedFileName != null)
            {
                ConsoleInfo($"unload main: {_mainLoadedSource} / {_mainLoadedFileName}");
                controller.CcReleaseTexture(_mainLoadedSource, _mainLoadedFileName);
            }

            _mainLoadedSource = source;
            _mainLoadedFileName = fileName;

            controller.LoadFile(this, _mainLoadedSource, _mainLoadedFileName, 100, _mainTextureLoadChannel);
            animator.SetBool(AnimatorIsLoading, true);

            // フォールバック: すでにキャッシュ済みなら即時反映してローディング解除
            var immediateTexture = controller.CcGetTexture(_mainLoadedSource, _mainLoadedFileName);
            if (immediateTexture != null)
            {
                animator.SetBool(AnimatorIsLoading, false);
                ApplyMainTexture(immediateTexture);
                _mainAppliedFromCache = true;
            }
        }

        public override void OnFileLoadSuccess(string sourceUrl, string fileUrl, string channel)
        {
            base.OnFileLoadSuccess(sourceUrl, fileUrl, channel);
            if (fileUrl == null) return;
            if (_mainTextureLoadChannel != channel) return;

            ConsoleDebug($"main slide image loaded: {fileUrl}");
            if (_mainLoadedFileName != fileUrl || _mainLoadedSource != sourceUrl)
            {
                ConsoleDebug($"main texture not match: {_mainLoadedFileName} / {_mainLoadedSource} != {fileUrl} / {sourceUrl}");
                return;
            }

            animator.SetBool(AnimatorIsLoading, false);
            if (_mainAppliedFromCache)
            {
                // すでにフォールバックで適用済みなら二重に使用カウントを増やさない
                _mainAppliedFromCache = false;
                return;
            }
            var texture = controller.CcGetTexture(sourceUrl, fileUrl);
            if (texture == null)
            {
                ConsoleError($"Failed to get texture for {sourceUrl}/{fileUrl}");
                return;
            }

            ApplyMainTexture(texture);
        }

        public override void OnFileLoadError(string sourceUrl, string fileUrl, string channel, LoadError error)
        {
            base.OnFileLoadError(sourceUrl, fileUrl, channel, error);
            if (_mainTextureLoadChannel != channel) return;
            if (fileUrl == null) return;
            // 現在要求中のファイルと一致する場合のみローディング解除
            if (_mainLoadedFileName != fileUrl || _mainLoadedSource != sourceUrl) return;
            animator.SetBool(AnimatorIsLoading, false);
            ConsoleError($"main slide image load error: {error} {sourceUrl}/{fileUrl}");
        }

        private void ApplyMainTexture(Texture2D texture)
        {
            if (texture == null) return;
            slideMainView.texture = texture;
            slideMainViewFitter.aspectRatio = (float)texture.width / texture.height;
        }
    }
}
