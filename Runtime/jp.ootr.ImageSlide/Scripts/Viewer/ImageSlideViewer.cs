using UdonSharp;

namespace jp.ootr.ImageSlide.Viewer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ImageSlideViewer : TransformLock
    {
        public override string GetClassName()
        {
            return "jp.ootr.ImageSlide.Viewer.ImageSlideViewer";
        }

        public override string GetDisplayName()
        {
            return "ImageSlideViewer";
        }
    }
}
