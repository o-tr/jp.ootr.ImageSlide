using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UISplashScreen : UISlide 
    {
        private readonly int _animatorSplash = Animator.StringToHash("Splash");
        public override void UrlsUpdated()
        {
            base.UrlsUpdated();
            animator.SetBool(_animatorSplash, imageSlide.slideCount == 0);
        }
    }
}