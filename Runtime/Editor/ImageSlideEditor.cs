using jp.ootr.common;
using jp.ootr.ImageDeviceController.Editor;
using UnityEditor;
using UnityEngine;

namespace jp.ootr.ImageSlide.Editor
{
    [CustomEditor(typeof(ImageSlide))]
    public class ImageSlideEditor : CommonDeviceEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Debug)
            {
                return;
            }
         
            var script = (ImageSlide)target;
            EditorGUILayout.Space();

        }

        public override void ShowScriptName()
        {
            EditorGUILayout.LabelField("ImageSlide", EditorStyle.UiTitle);
        }
    }
}