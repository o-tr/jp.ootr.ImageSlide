using UdonSharp;

namespace jp.ootr.ImageSlide
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ImageSlide : UIDeviceList
    {
        public override string GetClassName()
        {
            return "jp.ootr.ImageSlide.ImageSlide";
        }
        
        public override string GetDisplayName()
        {
            return "ImageSlide";
        }

        public override bool IsCastableDevice()
        {
            return false;
        }
    }
}