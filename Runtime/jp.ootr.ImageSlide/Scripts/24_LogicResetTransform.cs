using UnityEngine;
using VRC.SDK3.Components;

namespace jp.ootr.ImageSlide
{
    public class LogicResetTransform : LogicViewerSeekMode {
        [SerializeField] private VRCObjectSync rootObjectSync;
        [SerializeField] private GameObject nextPreviewTransformResetTarget;
        [SerializeField] private GameObject noteTransformResetTarget;
        [SerializeField] private GameObject thumbnailTransformResetTarget;
        private Vector3 _nextPreviewTransformResetPosition;
        private Quaternion _nextPreviewTransformResetRotation;
        private Vector3 _nextPreviewTransformResetScale;
        private Vector3 _noteTransformResetPosition;
        private Quaternion _noteTransformResetRotation;
        private Vector3 _noteTransformResetScale;
        private Vector3 _thumbnailTransformResetPosition;
        private Quaternion _thumbnailTransformResetRotation;
        private Vector3 _thumbnailTransformResetScale;
        
        public override void InitController()
        {
            base.InitController();
            _nextPreviewTransformResetPosition = nextPreviewTransformResetTarget.transform.localPosition;
            _nextPreviewTransformResetRotation = nextPreviewTransformResetTarget.transform.localRotation;
            _nextPreviewTransformResetScale = nextPreviewTransformResetTarget.transform.localScale;
            _noteTransformResetPosition = noteTransformResetTarget.transform.localPosition;
            _noteTransformResetRotation = noteTransformResetTarget.transform.localRotation;
            _noteTransformResetScale = noteTransformResetTarget.transform.localScale;
            _thumbnailTransformResetPosition = thumbnailTransformResetTarget.transform.localPosition;
            _thumbnailTransformResetRotation = thumbnailTransformResetTarget.transform.localRotation;
            _thumbnailTransformResetScale = thumbnailTransformResetTarget.transform.localScale;
        }
        
        public void ResetRootTransform()
        {
            rootObjectSync.Respawn();
        }
        
        public void ResetNextPreviewTransform()
        {
            nextPreviewTransformResetTarget.transform.localPosition = _nextPreviewTransformResetPosition;
            nextPreviewTransformResetTarget.transform.localRotation = _nextPreviewTransformResetRotation;
            nextPreviewTransformResetTarget.transform.localScale = _nextPreviewTransformResetScale;
        }
        
        public void ResetNoteTransform()
        {
            noteTransformResetTarget.transform.localPosition = _noteTransformResetPosition;
            noteTransformResetTarget.transform.localRotation = _noteTransformResetRotation;
            noteTransformResetTarget.transform.localScale = _noteTransformResetScale;
        }
        
        public void ResetThumbnailTransform()
        {
            thumbnailTransformResetTarget.transform.localPosition = _thumbnailTransformResetPosition;
            thumbnailTransformResetTarget.transform.localRotation = _thumbnailTransformResetRotation;
            thumbnailTransformResetTarget.transform.localScale = _thumbnailTransformResetScale;
        }
    }
}
