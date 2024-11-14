﻿using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class TransformLock : UISplashScreen {
        [SerializeField] internal Collider[] targetColliders;
        [SerializeField] internal GameObject transformLockButtonActiveIcon;
        [SerializeField] internal bool isTransformLocked = false;

        protected override void OnEnable()
        {
            base.OnEnable();
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