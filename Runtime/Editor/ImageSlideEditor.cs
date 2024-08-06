using System;
using System.Collections.Generic;
using System.Linq;
using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using jp.ootr.ImageDeviceController.CommonDevice;
using jp.ootr.ImageDeviceController.Editor;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;
using Object = UnityEngine.Object;

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

            BuildDeviceList(script);
            BuildDefinedUrls(script);
            
            if (GUILayout.Button("デバイスリストを更新"))
            {
                ImageSlideUtils.GenerateDeviceList(script);
                script.BuildSourceList();
            }
        }

        public override void ShowScriptName()
        {
            EditorGUILayout.LabelField("ImageSlide", EditorStyle.UiTitle);
        }
        
        private void BuildDeviceList(ImageSlide script)
        {
            EditorGUILayout.LabelField("TargetDevices", EditorStyles.boldLabel);
            var uuids = script.deviceSelectedUuids.ToList();
            var changed = false;

            using (new GUILayout.VerticalScope("box",GUILayout.MinHeight(150)))
            {
                foreach (var device in script.devices.GetCastableDevices())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        var isSelected = uuids.Contains(device.deviceUuid);
                        var newSelected = EditorGUILayout.ToggleLeft($"{device.deviceName} ({device.GetDisplayName()}/{device.deviceUuid})", isSelected);
                        if (isSelected != newSelected)
                        {
                            changed = true;
                            if (newSelected)
                            {
                                uuids.Add(device.deviceUuid);
                            }
                            else
                            {
                                uuids.Remove(device.deviceUuid);
                            }
                        }
                    }
                }
            }

            if (!changed) return;
            SerializedObject so = new SerializedObject(script);
            var property = so.FindProperty("deviceSelectedUuids");
            property.arraySize = uuids.Count;
            for (int i = 0; i < uuids.Count; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = uuids[i];
            }
            so.ApplyModifiedProperties();
            ImageSlideUtils.GenerateDeviceList(script);
            EditorUtility.SetDirty(script);
        }

        private void BuildDefinedUrls(ImageSlide script)
        {
            EditorGUI.BeginChangeCheck();
            var changed = false;
            
            EditorGUILayout.LabelField("Slide Urls", EditorStyles.boldLabel);
            var urlsLength = script.definedSources.Length;
            var urlOptionsLength = script.definedSourceOptions.Length;
            var arraySize = Mathf.Max(urlsLength, urlOptionsLength);
            if (urlsLength != arraySize || urlOptionsLength != arraySize)
            {
                Array.Resize(ref script.definedSourceOptions, arraySize);
                Array.Resize(ref script.definedSources, arraySize);
                changed = true;
            }

            using (new GUILayout.VerticalScope("box",GUILayout.MinHeight(150)))
            {
            
                for (int i = 0; i < script.definedSources.Length; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        script.definedSourceOptions[i].ParseSourceOptions(out var type, out var offset, out var interval);
                        EditorGUILayout.LabelField("Type", GUILayout.Width(50));
                        var newType = (URLType)EditorGUILayout.EnumPopup(type, GUILayout.Width(75));
                        if (newType != type)
                        {
                            type = newType;
                            if (type == URLType.Video)
                            {
                                offset = 0.5f;
                                interval = 1f;
                            }
                            changed = true;
                        }
                        EditorGUILayout.LabelField("Source", GUILayout.Width(75));
                        script.definedSources[i] = EditorGUILayout.TextField(script.definedSources[i]);
                        if (type == URLType.Video)
                        {
                            EditorGUILayout.LabelField("Offset", GUILayout.Width(50));
                            var newOffset = EditorGUILayout.FloatField(offset, GUILayout.Width(50));
                            EditorGUILayout.LabelField("Interval", GUILayout.Width(50));
                            var newInterval = EditorGUILayout.FloatField(interval, GUILayout.Width(50));
                            script.definedSourceOptions[i] = UrlUtil.BuildSourceOptions(type, newOffset, newInterval);
                        }
                        else
                        {
                            script.definedSourceOptions[i] = UrlUtil.BuildSourceOptions(type, offset, interval);
                        }

                        if (i > 0)
                        {
                            if (GUILayout.Button("↑", GUILayout.Width(25)))
                            {
                                var tmp = script.definedSources[i - 1];
                                script.definedSources[i - 1] = script.definedSources[i];
                                script.definedSources[i] = tmp;
                                tmp = script.definedSourceOptions[i - 1];
                                script.definedSourceOptions[i - 1] = script.definedSourceOptions[i];
                                script.definedSourceOptions[i] = tmp;
                                changed = true;
                            }
                        }
                        else
                        {
                            GUILayout.Space(25);
                        }
                        if (i < script.definedSources.Length - 1)
                        {
                            if (GUILayout.Button("↓", GUILayout.Width(25)))
                            {
                                var tmp = script.definedSources[i + 1];
                                script.definedSources[i + 1] = script.definedSources[i];
                                script.definedSources[i] = tmp;
                                tmp = script.definedSourceOptions[i + 1];
                                script.definedSourceOptions[i + 1] = script.definedSourceOptions[i];
                                script.definedSourceOptions[i] = tmp;
                                changed = true;
                            }
                        }
                        else
                        {
                            GUILayout.Space(25);
                        }
                        
                        if (GUILayout.Button("X", GUILayout.Width(25)))
                        {
                            ArrayUtility.RemoveAt(ref script.definedSources, i);
                            ArrayUtility.RemoveAt(ref script.definedSourceOptions, i);
                            changed = true;
                        }
                    }
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Image"))
                {
                    AddSource(script, "", UrlUtil.BuildSourceOptions(URLType.Image, 0, 0));
                    changed = true;
                }
                if (GUILayout.Button("Add TextZip"))
                {
                    AddSource(script, "", UrlUtil.BuildSourceOptions(URLType.TextZip, 0, 0));
                    changed = true;
                }
                if (GUILayout.Button("Add Video"))
                {
                    AddSource(script, "", UrlUtil.BuildSourceOptions(URLType.Video, 0.5f, 1f));
                    changed = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (!changed && !EditorGUI.EndChangeCheck()) return;
            SerializedObject so = new SerializedObject(script);
            so.FindProperty("definedSources").ApplyArray(script.definedSources);
            so.FindProperty("definedSourceOptions").ApplyArray(script.definedSourceOptions);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(script);
            script.BuildSourceList();
        }
        
        private void AddSource(ImageSlide script,string source, string options)
        {
            Array.Resize(ref script.definedSources, script.definedSources.Length + 1);
            Array.Resize(ref script.definedSourceOptions, script.definedSourceOptions.Length + 1);
            script.definedSources[script.definedSources.Length - 1] = source;
            script.definedSourceOptions[script.definedSourceOptions.Length - 1] = options;
        }
        public void OnValidate()
        {
            var script = (ImageSlide)target;
            ImageSlideUtils.GenerateDeviceList(script);
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
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var scripts = ComponentUtils.GetAllComponents<ImageSlide>();
                foreach (var script in scripts)
                {
                    ImageSlideUtils.GenerateDeviceList(script);
                }
            }
        }
    }
    
    public class SetObjectReferences : UnityEditor.Editor, IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 11;
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            var scripts = ComponentUtils.GetAllComponents<ImageSlide>();
            foreach (var script in scripts)
            {
                ImageSlideUtils.GenerateDeviceList(script);
            }
            return true;
        }
    }
    
    public static class ImageSlideUtils
    {
        public static CommonDevice[] GetCastableDevices(this CommonDevice[] devices)
        {
            return devices.Where((device => device.IsCastableDevice())).ToArray();
        }
        
        public static void GenerateDeviceList(ImageSlide script)
        {
            var rootObject = script.rootDeviceNameText.transform.parent.parent.gameObject;
            CleanUp(rootObject);
            Generate(script);
            script.settingsTransform.ToListChildrenVertical(24,0,true);
        }

        private static void CleanUp(GameObject rootObject)
        {
            var list = new List<GameObject>();
            foreach (Transform child in rootObject.transform)
            {
                if (child.gameObject.name.StartsWith("_")) continue;
                list.Add(child.gameObject);
            }

            foreach (var obj in list)
            {
                Object.DestroyImmediate(obj);
            }
        }

        private static void Generate(ImageSlide script)
        {
            var baseObject = script.rootDeviceNameText.transform.parent.gameObject;
            foreach (var device in script.devices.GetCastableDevices())
            {
                script.rootDeviceNameText.text = device.deviceName;
                script.rootDeviceIcon.texture = device.deviceIcon;
                script.rootDeviceToggle.isOn = script.deviceSelectedUuids.Contains(device.deviceUuid);
                var newObject = Object.Instantiate(baseObject, baseObject.transform.parent);
                newObject.name = device.deviceUuid;
                newObject.SetActive(true);
            }
            script.rootDeviceTransform.ToListChildrenVertical(24,24,true);
        }
    }
    
    public static class EditorUtils
    {
        public static void ApplyArray(this SerializedProperty property, string[] data)
        {
            property.arraySize = data.Length;
            for (var i = 0; i < data.Length; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = data[i];
            }
        }
    }
}