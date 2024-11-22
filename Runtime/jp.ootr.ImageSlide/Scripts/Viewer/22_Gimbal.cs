using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class Gimbal : TransformLock {
        [SerializeField] internal bool isGimbalEnabled;

        public override void OnDrop()
        {
            base.OnDrop();
            if (!isGimbalEnabled) return;
            GimbalUpdate();
        }

        private void GimbalUpdate()
        {
            var forwardVector = rootGameObject.transform.up * -1;
            Vector3 globalUp = Vector3.up;
            Quaternion targetRotation = Quaternion.LookRotation(forwardVector, globalUp);
            targetRotation *= Quaternion.Euler(-90, 0, 0);
            rootGameObject.transform.rotation = targetRotation;
        }
    }
}