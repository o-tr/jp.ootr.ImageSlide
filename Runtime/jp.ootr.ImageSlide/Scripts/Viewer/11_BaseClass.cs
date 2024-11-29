using JetBrains.Annotations;
using jp.ootr.ImageDeviceController.CommonDevice;
using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class BaseClass : CommonDevice
    {
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

        public virtual void ShowSyncingModal([CanBeNull] string content)
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
