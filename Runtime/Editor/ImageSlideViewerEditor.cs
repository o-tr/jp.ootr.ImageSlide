#if UNITY_EDITOR
using jp.ootr.common;
using jp.ootr.ImageSlide.Viewer;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.ootr.ImageSlide.Editor.Viewer
{
    [CustomEditor(typeof(ImageSlideViewer))]
    public class ImageSlideViewerEditor : UnityEditor.Editor
    {
        private bool _debug;
        private SerializedProperty _imageSlide;
        private SerializedProperty _isObjectSyncEnabled;
        private SerializedProperty _seekDisabled;

        public virtual void OnEnable()
        {
            _imageSlide = serializedObject.FindProperty("imageSlide");
            _seekDisabled = serializedObject.FindProperty("seekDisabled");
            _isObjectSyncEnabled = serializedObject.FindProperty("isObjectSyncEnabled");
        }

        public override void OnInspectorGUI()
        {
            _debug = EditorGUILayout.ToggleLeft("Debug", _debug);
            if (_debug)
            {
                base.OnInspectorGUI();
                return;
            }

            ShowScriptName();
            EditorGUILayout.Space();

            var script = (ImageSlideViewer)target;
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_imageSlide);
            serializedObject.ApplyModifiedProperties();

            if (script.imageSlide == null)
            {
                EditorGUILayout.Space();
                var content =
                    new GUIContent(
                        "Please assign this device to ImageSlide\n\nこのデバイスをImageSlideの管理対象に追加してください");
                content.image = EditorGUIUtility.IconContent("console.erroricon").image;
                EditorGUILayout.HelpBox(content);
            }

            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_seekDisabled, new GUIContent("Seek Disabled"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(_isObjectSyncEnabled, new GUIContent("Object Sync Enabled"));
            serializedObject.ApplyModifiedProperties();

            script.splashImage.texture =
                (Texture)EditorGUILayout.ObjectField("Splash Image", script.splashImage.texture, typeof(Texture),
                    false);


            if (!EditorGUI.EndChangeCheck()) return;
            script.SetSeekDisabled(script.seekDisabled);
            ImageSlideViewerUtils.UpdateObjectSync(script);

            EditorUtility.SetDirty(script);
        }

        private void ShowScriptName()
        {
            EditorGUILayout.LabelField("ImageSlideViewer", EditorStyle.UiTitle);
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
    }
}
#endif
