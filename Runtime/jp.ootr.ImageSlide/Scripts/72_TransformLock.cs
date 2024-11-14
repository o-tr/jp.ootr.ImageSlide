using UnityEngine;

namespace jp.ootr.ImageSlide
{
    public class TransformLock : Module {
        [SerializeField] private Collider[] targetColliders;
        [SerializeField] private GameObject transformLockButtonActiveIcon;
        [SerializeField] internal bool isTransformLocked = false;
        
        public override void InitController()
        {
            base.InitController();
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
            foreach (var targetCollider in targetColliders)
            {
                targetCollider.enabled = !isTransformLocked;
            }
        }
    }
}
