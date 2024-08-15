using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class BaseClass : common.BaseClass
    {
        [SerializeField] public RawImage splashImage;
        [SerializeField] public AspectRatioFitter splashImageFitter;

        public virtual void UrlsUpdated()
        {
        }

        public virtual void IndexUpdated(int index)
        {
        }

        public virtual void InitImageSlide()
        {
        }

        public virtual void ShowSyncingModal(string content)
        {
        }

        public virtual void HideSyncingModal()
        {
        }
    }
}