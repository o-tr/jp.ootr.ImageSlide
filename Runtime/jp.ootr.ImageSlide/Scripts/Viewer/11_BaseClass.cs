using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class BaseClass : common.BaseClass
    {
        [SerializeField] public RawImage splashImage;
        [SerializeField] public AspectRatioFitter splashImageFitter;
        [SerializeField] public Texture2D splashImageTexture;
        [SerializeField] public bool isObjectSyncEnabled;
        [SerializeField] public GameObject rootGameObject;

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
        
        public virtual void UpdateSeekMode(SeekMode seekMode)
        {
            
        }
    }
}
