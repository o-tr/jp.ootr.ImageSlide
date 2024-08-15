using jp.ootr.common;
using jp.ootr.ImageSlide.Viewer;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.ootr.ImageSlide.Editor
{
    [CustomEditor(typeof(ImageSlideViewer))]
    public class ImageSlideViewerEditor : UnityEditor.Editor
    {
        private bool _debug;
        private SerializedProperty _imageSlide;
        
        public virtual void OnEnable()
        {
            _imageSlide = serializedObject.FindProperty("imageSlide");
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

            script.splashImage.texture =
                (Texture)EditorGUILayout.ObjectField("Splash Image", script.splashImage.texture, typeof(Texture),
                    false);


            if (!EditorGUI.EndChangeCheck()) return;

            EditorUtility.SetDirty(script);
        }

        private void ShowScriptName()
        {
            EditorGUILayout.LabelField("ImageSlideViewer", EditorStyle.UiTitle);
        }
    }

    [InitializeOnLoad]
    public class PlayModeNotifier_ImageSlideviewer
    {
        static PlayModeNotifier_ImageSlideviewer()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                var imageSlideViewer = ComponentUtils.GetAllComponents<ImageSlideViewer>();

                ImageSlideViewerUtils.ValidateImageSlide(imageSlideViewer.ToArray());
            }
        }
    }

    public class SetObjectReferences_ImageSlideViewer : UnityEditor.Editor, IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 12;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            var imageSlideViewer = ComponentUtils.GetAllComponents<ImageSlideViewer>();

            return ImageSlideViewerUtils.ValidateImageSlide(imageSlideViewer.ToArray());
        }
    }

    public static class ImageSlideViewerUtils
    {
        public static bool ValidateImageSlide(ImageSlideViewer[] imageSlideViewers)
        {
            var flag = true;
            foreach (var viewer in imageSlideViewers)
            {
                if (viewer.imageSlide == null)
                {
                    Debug.LogWarning("ImageSlideViewer: ImageSlide is not assigned");
                    flag = false;
                    continue;
                }

                if (viewer.imageSlide.listeners.Has(viewer)) continue;
                viewer.imageSlide.listeners = viewer.imageSlide.listeners.Append(viewer);
                EditorUtility.SetDirty(viewer.imageSlide);
            }

            return flag;
        }
    }
}