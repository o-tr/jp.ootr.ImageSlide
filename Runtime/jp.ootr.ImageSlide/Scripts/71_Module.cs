using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class Module : EventHandler 
    {
        [SerializeField] private GameObject thumbnailButtonActiveIcon;
        [SerializeField] private GameObject thumbnailRoot;
        
        [SerializeField] private GameObject noteButtonActiveIcon;
        [SerializeField] private GameObject noteRoot;
        
        [SerializeField] private GameObject nextPreviewButtonActiveIcon;
        [SerializeField] private GameObject nextPreviewRoot;
        
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
