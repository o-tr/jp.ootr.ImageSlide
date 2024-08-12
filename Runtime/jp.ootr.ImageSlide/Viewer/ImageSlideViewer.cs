using jp.ootr.common;
using UdonSharp;
using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ImageSlideViewer : UISplashScreen
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