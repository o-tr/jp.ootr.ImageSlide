using UnityEngine;

namespace jp.ootr.ImageSlide
{
    public class Gimbal : TransformLock
    {
        [SerializeField] private GameObject rootGameObject;
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
            var globalUp = Vector3.up;
            var targetRotation = Quaternion.LookRotation(forwardVector, globalUp);
            targetRotation *= Quaternion.Euler(-90, 0, 0);
            rootGameObject.transform.rotation = targetRotation;
        }
    }
}
