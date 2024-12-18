using jp.ootr.ImageSlide.Viewer;

namespace jp.ootr.ImageSlide
{
    public class EventHandler : UIClock
    {
        public ImageSlideViewer[] listeners = new ImageSlideViewer[0];

        public override void InitController()
        {
            base.InitController();
            foreach (var listener in listeners) listener.InitImageSlide();
        }

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
        
        protected override void ShowErrorModal(string title, string description)
        {
            base.ShowErrorModal(title, description);
            foreach (var listener in listeners) listener.ShowErrorModal(title, description);
        }
        
        public override void CloseErrorModal()
        {
            base.CloseErrorModal();
            foreach (var listener in listeners) listener.CloseErrorModal();
        }

        protected override void SeekModeChanged(SeekMode mode)
        {
            base.SeekModeChanged(mode);
            foreach (var listener in listeners) listener.SeekModeChanged(mode);
        }
    }
}
