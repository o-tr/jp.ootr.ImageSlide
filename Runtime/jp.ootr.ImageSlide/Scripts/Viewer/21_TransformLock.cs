using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class TransformLock : UISplashScreen
    {
        [SerializeField] private Collider[] targetColliders;
        [SerializeField] private GameObject transformLockButtonActiveIcon;
        [SerializeField] private bool isTransformLocked;

        public override void InitImageSlide()
        {
            base.InitImageSlide();
            ApplyTransformLock();
        }

        public void OnLockToggle()
        {
            isTransformLocked = !isTransformLocked;
            ApplyTransformLock();
        }

        private void ApplyTransformLock()
        {
            transformLockButtonActiveIcon.SetActive(isTransformLocked);
            foreach (var targetCollider in targetColliders) targetCollider.enabled = !isTransformLocked;
        }
    }
}
