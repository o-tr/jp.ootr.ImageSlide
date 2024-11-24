using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class BaseClass : common.BaseClass
    {
        [SerializeField] private RawImage splashImage;
        [SerializeField] private AspectRatioFitter splashImageFitter;
        [SerializeField] private Texture2D splashImageTexture;
        [SerializeField] internal bool isObjectSyncEnabled;
        [SerializeField] internal GameObject rootGameObject;

        public virtual void UrlsUpdated()
        {
        }

        public virtual void IndexUpdated(int index)
        {
        }

        public virtual void InitImageSlide()
        {
        }

        public virtual void ShowSyncingModal([CanBeNull]string content)
        {
        }

        public virtual void HideSyncingModal()
        {
        }

        public virtual void SeekModeChanged(SeekMode seekMode)
        {
        }
    }
}
