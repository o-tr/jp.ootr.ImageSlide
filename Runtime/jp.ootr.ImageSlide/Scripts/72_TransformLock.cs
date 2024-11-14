using UnityEngine;

namespace jp.ootr.ImageSlide
{
    public class TransformLock : Module {
        [SerializeField] private Collider rootCollider;
        [SerializeField] private GameObject rootTransformLockButtonIcon;
        [SerializeField] internal bool rootTransformLocked = false;
        [SerializeField] private Collider nextPreviewCollider;
        [SerializeField] private GameObject nextPreviewTransformLockButtonIcon;
        [SerializeField] internal bool nextPreviewTransformLocked = false;
        [SerializeField] private Collider noteCollider;
        [SerializeField] private GameObject noteTransformLockButtonIcon;
        [SerializeField] internal bool noteTransformLocked = false;
        [SerializeField] private Collider thumbnailCollider;
        [SerializeField] private GameObject thumbnailTransformLockButtonIcon;
        [SerializeField] internal bool thumbnailTransformLocked = false;
        
        public override void InitController()
        {
            base.InitController();
            rootCollider.enabled = !rootTransformLocked;
            rootTransformLockButtonIcon.SetActive(rootTransformLocked);
            nextPreviewCollider.enabled = !nextPreviewTransformLocked;
            nextPreviewTransformLockButtonIcon.SetActive(nextPreviewTransformLocked);
            noteCollider.enabled = !noteTransformLocked;
            noteTransformLockButtonIcon.SetActive(noteTransformLocked);
            thumbnailCollider.enabled = !thumbnailTransformLocked;
            thumbnailTransformLockButtonIcon.SetActive(thumbnailTransformLocked);
        }
        
        public void OnRootLockToggle()
        {
            rootTransformLocked = !rootTransformLocked;
            rootCollider.enabled = !rootTransformLocked;
            rootTransformLockButtonIcon.SetActive(rootTransformLocked);
        }
        
        public void OnNextPreviewLockToggle()
        {
            nextPreviewTransformLocked = !nextPreviewTransformLocked;
            nextPreviewCollider.enabled = !nextPreviewTransformLocked;
            nextPreviewTransformLockButtonIcon.SetActive(nextPreviewTransformLocked);
        }
        
        public void OnNoteLockToggle()
        {
            noteTransformLocked = !noteTransformLocked;
            noteCollider.enabled = !noteTransformLocked;
            noteTransformLockButtonIcon.SetActive(noteTransformLocked);
        }
        
        public void OnThumbnailLockToggle()
        {
            thumbnailTransformLocked = !thumbnailTransformLocked;
            thumbnailCollider.enabled = !thumbnailTransformLocked;
            thumbnailTransformLockButtonIcon.SetActive(thumbnailTransformLocked);
        }
    }
}
