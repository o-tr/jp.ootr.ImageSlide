using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class CustomScaler : UISplashScreen {
        private int _customScale = 10;
        [SerializeField] internal int minScale = 1;
        [SerializeField] internal int maxScale = 20;
        [SerializeField] internal int defaultScale = 10;
        [SerializeField] internal int scaleResolution = 10;
        [SerializeField] internal GameObject[] scaleTargetGameObject;
        [SerializeField] internal GameObject[] reverseScaleTargetGameObject;
        
        public void ScaleUp()
        {
            _customScale += 1;
            ApplyScale();
        }
        
        public void ScaleDown()
        {
            _customScale -= 1;
            ApplyScale();
        }
        
        public void ResetScale()
        {
            _customScale = defaultScale;
            ApplyScale();
        }
        
        private void ApplyScale()
        {
            _customScale = Mathf.Clamp(_customScale, minScale, maxScale);
            var scale = _customScale / (float)scaleResolution;
            var reverseScale = 1 / scale;
            foreach (var obj in scaleTargetGameObject)
            {
                if (obj == null) continue;
                obj.transform.localScale = new Vector3(scale, scale, scale);
            }
            foreach (var obj in reverseScaleTargetGameObject)
            {
                if (obj == null) continue;
                obj.transform.localScale = new Vector3(reverseScale, reverseScale, reverseScale);
            }
        }
    }
}
