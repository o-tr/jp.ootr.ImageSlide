using jp.ootr.ImageSlide.Viewer;

namespace jp.ootr.ImageSlide
{
    public class EventHandler : UIStopWatch
    {
        public ImageSlideViewer[] listeners = new ImageSlideViewer[0];

        protected override void UrlsUpdated()
        {
            base.UrlsUpdated();
            foreach (var listener in listeners) listener.UrlsUpdated();
        }

        protected override void IndexUpdated(int index)
        {
            base.IndexUpdated(index);
            foreach (var listener in listeners) listener.IndexUpdated(index);
        }

        protected override void ShowSyncingModal(string content)
        {
            base.ShowSyncingModal(content);
            foreach (var listener in listeners) listener.ShowSyncingModal(content);
        }

        protected override void HideSyncingModal()
        {
            base.HideSyncingModal();
            foreach (var listener in listeners) listener.HideSyncingModal();
        }
    }
}
