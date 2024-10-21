#if UNITY_EDITOR
using jp.ootr.common;
using jp.ootr.common.Editor;
using jp.ootr.ImageDeviceController.Editor;
using jp.ootr.ImageSlide.Viewer;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using VRC.SDK3.Components;
using VRC.SDKBase.Editor.BuildPipeline;
using Toggle = UnityEngine.UIElements.Toggle;

namespace jp.ootr.ImageSlide.Editor.Viewer
{
    [CustomEditor(typeof(ImageSlideViewer))]
    public class ImageSlideViewerEditor : BaseEditor
    {
        private SerializedProperty _imageSlide;
        private SerializedProperty _splashImage;
        private SerializedProperty _splashImageFitter;
        
        public override void OnEnable()
        {
            base.OnEnable();   
            _imageSlide = serializedObject.FindProperty("imageSlide");
            _splashImage = serializedObject.FindProperty("splashImage");
            _splashImageFitter = serializedObject.FindProperty("splashImageFitter");
        }
        
        protected override VisualElement GetLayout()
        {
            var container = new VisualElement();
            container.AddToClassList("container");
            container.Add(ShowImageSlidePicker());
            container.Add(ShowSeekDisabled());
            container.Add(ShowObjectSyncEnabled());
            container.Add(GetOther());
            return container;
        }
        
        private VisualElement ShowImageSlidePicker()
        {
            var error = new HelpBox("参照先のImageSlideを設定してください\nPlease assign this device to ImageSlide",HelpBoxMessageType.Error);
            var slide = new ObjectField("ImageSlide")
            {
                bindingPath = "imageSlide",
                objectType = typeof(ImageSlide)
            };
            var hasError = _imageSlide.objectReferenceValue == null;
            if (_imageSlide.objectReferenceValue == null)
            {
                InfoBlock.Add(error);
            }
            slide.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null && !hasError)
                {
                    InfoBlock.Add(error);
                    hasError = true;
                }
                else if (evt.newValue != null && hasError)
                {
                    InfoBlock.Remove(error);
                    hasError = false;
                }
            });
            return slide;
        }
        
        private VisualElement ShowSeekDisabled()
        {
            var seekDisabled = new Toggle("Seek Disabled")
            {
                bindingPath = "seekDisabled"
            };
            seekDisabled.RegisterValueChangedCallback(evt =>
            {
                serializedObject.ApplyModifiedProperties();
                ImageSlideViewerUtils.UpdateSeekDisabled((ImageSlideViewer)target);
                serializedObject.Update();
            });
            return seekDisabled;
        }
        
        private VisualElement ShowObjectSyncEnabled()
        {
            var objectSyncEnabled = new Toggle("Object Sync Enabled")
            {
                bindingPath = "isObjectSyncEnabled"
            };
            objectSyncEnabled.RegisterValueChangedCallback(evt =>
            {
                serializedObject.ApplyModifiedProperties();
                ImageSlideViewerUtils.UpdateObjectSync((ImageSlideViewer)target);
                serializedObject.Update();
            });
            return objectSyncEnabled;
        }

        private VisualElement GetOther()
        {
            var foldout = new Foldout()
            {
                text = "Other",
                value = false
            };
            foldout.Add(ShowSplashImage());
            return foldout;
        }
        
        private VisualElement ShowSplashImage()
        {
            var splashImage = new ObjectField("Splash Image")
            {
                bindingPath = "splashImageTexture",
                objectType = typeof(Texture2D)
            };
            splashImage.RegisterValueChangedCallback(evt =>
            {
                var texture = evt.newValue as Texture2D;
                var image = (RawImage)_splashImage.objectReferenceValue;
                var soImage = new SerializedObject(image);
                soImage.Update();
                soImage.FindProperty("m_Texture").objectReferenceValue = texture;
                soImage.ApplyModifiedProperties();
                var aspectRatio = (AspectRatioFitter)_splashImageFitter.objectReferenceValue;
                var soFitter = new SerializedObject(aspectRatio);
                soFitter.Update();
                soFitter.FindProperty("m_AspectRatio").floatValue = (float)texture.width / texture.height;
                soFitter.ApplyModifiedProperties();
            });
            return splashImage;
        }

        protected override string GetScriptName()
        {
            return "ImageSlideViewer";
        }
    }

    [InitializeOnLoad]
    public class PlayModeNotifier
    {
        static PlayModeNotifier()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                var imageSlideViewer = ComponentUtils.GetAllComponents<ImageSlideViewer>();

                ImageSlideViewerUtils.ValidateImageSlides(imageSlideViewer.ToArray());
            }
        }
    }

    public class SetObjectReferences : UnityEditor.Editor, IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 12;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            var imageSlideViewer = ComponentUtils.GetAllComponents<ImageSlideViewer>();

            return ImageSlideViewerUtils.ValidateImageSlides(imageSlideViewer.ToArray());
        }
    }

    public static class ImageSlideViewerUtils
    {
        public static bool ValidateImageSlides(ImageSlideViewer[] imageSlideViewers)
        {
            var flag = true;
            foreach (var viewer in imageSlideViewers)
                if (!ValidateImageSlide(viewer))
                    flag = false;

            return flag;
        }

        public static bool ValidateImageSlide(ImageSlideViewer imageSlideViewer)
        {
            if (imageSlideViewer.imageSlide == null)
            {
                Debug.LogWarning("ImageSlideViewer: ImageSlide is not assigned");
                return false;
            }

            UpdateObjectSync(imageSlideViewer);
            UpdateSeekDisabled(imageSlideViewer);
            
            if (imageSlideViewer.imageSlide.listeners.Has(imageSlideViewer)) return true;
            imageSlideViewer.imageSlide.listeners = imageSlideViewer.imageSlide.listeners.Append(imageSlideViewer);
            EditorUtility.SetDirty(imageSlideViewer.imageSlide);
            return true;
        }

        public static void UpdateObjectSync(ImageSlideViewer imageSlideViewer)
        {
            var currentSyncObj = imageSlideViewer.rootGameObject.GetComponent<VRCObjectSync>();
            if (imageSlideViewer.isObjectSyncEnabled)
            {
                if (currentSyncObj == null) imageSlideViewer.rootGameObject.AddComponent<VRCObjectSync>();
            }
            else
            {
                if (currentSyncObj != null) Object.DestroyImmediate(currentSyncObj);
            }
        }

        public static void UpdateSeekDisabled(ImageSlideViewer imageSlideViewer)
        {
            imageSlideViewer.SetSeekDisabled(imageSlideViewer.seekDisabled);
        }
    }
}
#endif
