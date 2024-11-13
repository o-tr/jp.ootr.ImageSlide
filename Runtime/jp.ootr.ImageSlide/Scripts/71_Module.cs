using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class Module : EventHandler 
    {
        [SerializeField] internal GameObject thumbnailButtonActiveIcon;
        [SerializeField] internal GameObject thumbnailRoot;
        
        [SerializeField] internal GameObject noteButtonActiveIcon;
        [SerializeField] internal GameObject noteRoot;
        
        [SerializeField] internal GameObject nextPreviewButtonActiveIcon;
        [SerializeField] internal GameObject nextPreviewRoot;
        
        public void OnThumbnailToggle()
        {
            var active = thumbnailRoot.activeSelf;
            thumbnailRoot.SetActive(!active);
            thumbnailButtonActiveIcon.SetActive(!active);
        }
        
        public void OnNoteToggle()
        {
            var active = noteRoot.activeSelf;
            noteRoot.SetActive(!active);
            noteButtonActiveIcon.SetActive(!active);
        }
        
        public void OnNextPreviewToggle()
        {
            var active = nextPreviewRoot.activeSelf;
            nextPreviewRoot.SetActive(!active);
            nextPreviewButtonActiveIcon.SetActive(!active);
        }
    }
}
