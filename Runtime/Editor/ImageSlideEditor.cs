#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using jp.ootr.ImageDeviceController.CommonDevice;
using jp.ootr.ImageDeviceController.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase.Editor.BuildPipeline;
using Object = UnityEngine.Object;

namespace jp.ootr.ImageSlide.Editor
{
    [CustomEditor(typeof(ImageSlide))]
    public class ImageSlideEditor : CommonDeviceEditor
    {
        [SerializeField] private StyleSheet imageSlideStyle;
        private SerializedProperty _definedSourceTypes;
        private SerializedProperty _definedSourceOffsets;
        private SerializedProperty _definedSourceIntervals;
        private SerializedProperty _definedSources;
        private SerializedProperty _deviceSelectedUuids;
        
        private VisualElement _definedSourceContainer;
        private List<VisualElement> _definedSourceElements = new List<VisualElement>();

        public override void OnEnable()
        {
            base.OnEnable();
            _deviceSelectedUuids = serializedObject.FindProperty("deviceSelectedUuids");
            _definedSources = serializedObject.FindProperty("definedSources");
            _definedSourceTypes = serializedObject.FindProperty("definedSourceTypes");
            _definedSourceOffsets = serializedObject.FindProperty("definedSourceOffsets");
            _definedSourceIntervals = serializedObject.FindProperty("definedSourceIntervals");
            Root.styleSheets.Add(imageSlideStyle);
        }

        public void OnValidate()
        {
            var script = (ImageSlide)target;
            ImageSlideUtils.GenerateDeviceList(script);
        }

        protected override VisualElement GetContentTk()
        {
            var container = new VisualElement();
            container.AddToClassList("container");
            container.Add(BuildDeviceList((ImageSlide)target));
            container.Add(BuildDefinedUrls((ImageSlide)target));
            var transformLockToggle = new Toggle("Transform Lock")
            {
                bindingPath = nameof(ImageSlide.isTransformLocked),
            };
            container.Add(transformLockToggle);
            return container;
        }

        protected override void ShowContent()
        {
        }

        protected override string GetScriptName()
        {
            return "ImageSlide";
        }

        private VisualElement BuildDeviceList(ImageSlide script)
        {
            var container = new VisualElement();
            var label = new Label("TargetDevices");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(label);

            var scrollView = new VisualElement();
            scrollView.AddToClassList("list");
            container.Add(scrollView);

            var uuids = script.deviceSelectedUuids.ToList();

            foreach (var device in script.devices.GetCastableDevices())
            {
                var isSelected = uuids.Contains(device.deviceUuid);
            
                var deviceContainer = new VisualElement();
                deviceContainer.style.flexDirection = FlexDirection.Row;

                var toggle = new Toggle($"{device.deviceName} ({device.GetDisplayName()}/{device.deviceUuid})")
                {
                    value = isSelected,
                    style =
                    {
                        flexDirection = FlexDirection.RowReverse,
                        textOverflow = TextOverflow.Ellipsis,
                    }
                };
            
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        uuids.Add(device.deviceUuid);
                    else
                        uuids.Remove(device.deviceUuid);
                    
                    serializedObject.Update();
                    _deviceSelectedUuids.arraySize = uuids.Count;
                    for (var i = 0; i < uuids.Count; i++)
                        _deviceSelectedUuids.GetArrayElementAtIndex(i).stringValue = uuids[i];

                    serializedObject.ApplyModifiedProperties();
                    ImageSlideUtils.GenerateDeviceList(script);
                    EditorUtility.SetDirty(script);
                });

                deviceContainer.Add(toggle);
                scrollView.Add(deviceContainer);
            }
            
            var lockToggle = new Toggle("Lock Device List")
            {
                bindingPath = nameof(ImageSlide.isDeviceListLocked),
            };
            container.Add(lockToggle);

            return container;
        }
        
        private VisualElement BuildDefinedUrls(ImageSlide script)
        {
            var container = new VisualElement();
            var title = new Label("Slide Urls");
            title.AddToClassList("bold-label");
            container.Add(title);
            var urlsLength = script.definedSources.Length;
            var urlTypesLength = script.definedSourceTypes.Length;
            var urlOffsetsLength = script.definedSourceOffsets.Length;
            var urlIntervalsLength = script.definedSourceIntervals.Length;
            var arraySize = Mathf.Max(urlsLength, urlTypesLength, urlOffsetsLength, urlIntervalsLength);
            
            
            if (urlsLength != arraySize || urlTypesLength != arraySize || urlOffsetsLength != arraySize || urlIntervalsLength != arraySize)
            {
                serializedObject.Update();
                _definedSources.arraySize = arraySize;
                _definedSourceTypes.arraySize = arraySize;
                _definedSourceOffsets.arraySize = arraySize;
                _definedSourceIntervals.arraySize = arraySize;
                serializedObject.ApplyModifiedProperties();
            }

            var list = new VisualElement();
            list.AddToClassList("list");
            container.Add(list);
            _definedSourceContainer = list;
            RebuildTable();

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            container.Add(buttonContainer);

            var addImageButton = new Button(() =>
            {
                AddSource(script, "", UrlUtil.BuildSourceOptions(URLType.Image, 0, 0));
            }) { text = "Add Image" };
            buttonContainer.Add(addImageButton);

            var addTextZipButton = new Button(() =>
            {
                AddSource(script, "", UrlUtil.BuildSourceOptions(URLType.TextZip, 0, 0));
            }) { text = "Add TextZip" };
            buttonContainer.Add(addTextZipButton);

            var addVideoButton = new Button(() =>
            {
                AddSource(script, "", UrlUtil.BuildSourceOptions(URLType.Video, 0.5f, 1f));
            }) { text = "Add Video" };
            buttonContainer.Add(addVideoButton);

            serializedObject.ApplyModifiedProperties();

            return container;
        }

        private void RebuildTable()
        {
            _definedSourceContainer.Clear();
            _definedSourceElements.Clear();
            var script = (ImageSlide)target;
            for (var i = 0; i < script.definedSources.Length; i++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                _definedSourceContainer.Add(row);
                _definedSourceElements.Add(row);
                RebuildRow(i);
            }
            _definedSourceContainer.MarkDirtyRepaint();
        }
        
        private void RebuildRow(int index)
        {
            var script = (ImageSlide)target;
            var row = _definedSourceElements[index];
            row.Clear();
            
            var type = script.definedSourceTypes[index];
            
            var typeField = new EnumField("Type")
            {
                bindingPath = "definedSourceTypes.Array.data[" + index + "]",
            };
            typeField.Bind(serializedObject);
            typeField.RegisterValueChangedCallback(evt =>
            {
                RebuildRow(index);
            });
            typeField.AddToClassList("enum-field");
            row.Add(typeField);

            var sourceField = new TextField("Source")
            {
                bindingPath = "definedSources.Array.data[" + index + "]",
            };
            sourceField.Bind(serializedObject);
            sourceField.AddToClassList("text-field");
            row.Add(sourceField);

            if (type == URLType.Video)
            {
                var offsetField = new FloatField("Offset")
                {
                    bindingPath = "definedSourceOffsets.Array.data[" + index + "]",
                };
                offsetField.Bind(serializedObject);
                offsetField.AddToClassList("float-field");
                row.Add(offsetField);

                var intervalField = new FloatField("Interval")
                {
                    bindingPath = "definedSourceIntervals.Array.data[" + index + "]",
                };
                intervalField.Bind(serializedObject);
                intervalField.AddToClassList("float-field");
                row.Add(intervalField);
            }

            if (index > 0)
            {
                var upButton = new Button(() =>
                {
                    SwitchSource(index, index - 1);
                })
                { text = "↑", style = { width = 25 } };
                row.Add(upButton);
            }

            if (index < script.definedSources.Length - 1)
            {
                var downButton = new Button(() =>
                {
                    SwitchSource(index, index + 1);
                })
                { text = "↓", style = { width = 25 } };
                row.Add(downButton);
            }

            var deleteButton = new Button(() =>
            {
                serializedObject.Update();
                _definedSources.DeleteArrayElementAtIndex(index);
                _definedSourceTypes.DeleteArrayElementAtIndex(index);
                _definedSourceOffsets.DeleteArrayElementAtIndex(index);
                _definedSourceIntervals.DeleteArrayElementAtIndex(index);
                _definedSourceElements.RemoveAt(index);
                serializedObject.ApplyModifiedProperties();
                
                RebuildTable();
            })
            { text = "X", style = { width = 25 } };
            row.Add(deleteButton);
        }
        
        private void SwitchSource(int index1, int index2)
        {
            serializedObject.Update();
            (_definedSources.GetArrayElementAtIndex(index1).stringValue, _definedSources.GetArrayElementAtIndex(index2).stringValue) = 
                (_definedSources.GetArrayElementAtIndex(index2).stringValue, _definedSources.GetArrayElementAtIndex(index1).stringValue);
            (_definedSourceTypes.GetArrayElementAtIndex(index1).enumValueIndex, _definedSourceTypes.GetArrayElementAtIndex(index2).enumValueIndex) =
                (_definedSourceTypes.GetArrayElementAtIndex(index2).enumValueIndex, _definedSourceTypes.GetArrayElementAtIndex(index1).enumValueIndex);
            (_definedSourceOffsets.GetArrayElementAtIndex(index1).floatValue, _definedSourceOffsets.GetArrayElementAtIndex(index2).floatValue) =
                (_definedSourceOffsets.GetArrayElementAtIndex(index2).floatValue, _definedSourceOffsets.GetArrayElementAtIndex(index1).floatValue);
            (_definedSourceIntervals.GetArrayElementAtIndex(index1).floatValue, _definedSourceIntervals.GetArrayElementAtIndex(index2).floatValue) =
                (_definedSourceIntervals.GetArrayElementAtIndex(index2).floatValue, _definedSourceIntervals.GetArrayElementAtIndex(index1).floatValue);
            serializedObject.ApplyModifiedProperties();
            RebuildRow(index1);
            RebuildRow(index2);
        }

        private void AddSource(ImageSlide script, string source, string options)
        {
            options.ParseSourceOptions(out var type, out var offset, out var interval);
            serializedObject.Update();
            _definedSources.InsertArrayElementAtIndex(_definedSources.arraySize);
            _definedSourceTypes.InsertArrayElementAtIndex(_definedSourceTypes.arraySize);
            _definedSourceOffsets.InsertArrayElementAtIndex(_definedSourceOffsets.arraySize);
            _definedSourceIntervals.InsertArrayElementAtIndex(_definedSourceIntervals.arraySize);

            _definedSources.GetArrayElementAtIndex(_definedSources.arraySize - 1).stringValue = source;
            _definedSourceTypes.GetArrayElementAtIndex(_definedSourceTypes.arraySize - 1).enumValueIndex = (int)type;
            _definedSourceOffsets.GetArrayElementAtIndex(_definedSourceOffsets.arraySize - 1).floatValue = offset;
            _definedSourceIntervals.GetArrayElementAtIndex(_definedSourceIntervals.arraySize - 1).floatValue = interval;

            serializedObject.ApplyModifiedProperties();
            
            
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            _definedSourceContainer.Add(row);
            _definedSourceElements.Add(row);
            if (_definedSourceElements.Count > 1) RebuildRow(_definedSourceElements.Count - 2);
            RebuildRow(_definedSourceElements.Count - 1);
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
                foreach (var script in scripts) ImageSlideUtils.GenerateDeviceList(script);

                ImageSlideUtils.ValidateViewer(scripts.ToArray());
            }
        }
    }

    public class SetObjectReferences : UnityEditor.Editor, IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 11;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            var scripts = ComponentUtils.GetAllComponents<ImageSlide>();
            foreach (var script in scripts) ImageSlideUtils.GenerateDeviceList(script);

            return ImageSlideUtils.ValidateViewer(scripts.ToArray());
        }
    }

    public static class ImageSlideUtils
    {
        public static CommonDevice[] GetCastableDevices(this CommonDevice[] devices)
        {
            return devices.Where(device => device != null && device.IsCastableDevice()).ToArray();
        }

        public static void GenerateDeviceList(ImageSlide script)
        {
            var rootObject = script.rootDeviceNameText.transform.parent.parent.gameObject;
            rootObject.transform.ClearChildren();
            Generate(script);
            script.settingsTransform.ToListChildrenVertical(24, 0, true);
        }

        private static void Generate(ImageSlide script)
        {
            var baseObject = script.rootDeviceNameText.transform.parent.gameObject;
            var toggleList = new List<UnityEngine.UI.Toggle>();
            foreach (var device in script.devices.GetCastableDevices())
            {
                script.rootDeviceNameText.text = device.deviceName;
                script.rootDeviceIcon.texture = device.deviceIcon;
                script.rootDeviceToggle.isOn = script.deviceSelectedUuids.Contains(device.deviceUuid);
                var newObject = Object.Instantiate(baseObject, baseObject.transform.parent);
                newObject.name = device.deviceUuid;
                toggleList.Add(newObject.GetComponent<UnityEngine.UI.Toggle>());
                newObject.SetActive(true);
            }

            script.rootDeviceTransform.ToListChildrenVertical(24, 24, true);
        }

        public static bool ValidateViewer(ImageSlide[] slides)
        {
            var processedViewer = new List<ImageSlide>();
            var flag = true;
            foreach (var slide in slides)
            {
                slide.listeners = slide.listeners.Where(listener => listener != null).ToArray();
                var changed = false;

                foreach (var listener in slide.listeners)
                {
                    if (listener.imageSlide == slide) continue;
                    if (processedViewer.Contains(listener.imageSlide))
                    {
                        Debug.LogWarning(
                            $"ImageSlide: {listener.name} is already assigned to {listener.imageSlide.name}");
                        flag = false;
                        continue;
                    }

                    listener.imageSlide = slide;
                    changed = true;
                    processedViewer.Add(listener.imageSlide);
                }

                if (!changed) continue;
                EditorUtility.SetDirty(slide);
            }

            return flag;
        }
    }

    public static class EditorUtils
    {
        public static void ApplyArray(this SerializedProperty property, string[] data)
        {
            property.arraySize = data.Length;
            for (var i = 0; i < data.Length; i++) property.GetArrayElementAtIndex(i).stringValue = data[i];
        }
    }
}
#endif
