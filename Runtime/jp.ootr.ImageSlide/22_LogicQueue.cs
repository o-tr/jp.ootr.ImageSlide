using System;
using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace jp.ootr.ImageSlide
{
    public class LogicQueue : LogicSync
    {
        private string[] _queue = new string[0];
        private bool _isProcessing;

        protected string[] Sources = new string[0];
        protected string[] Options = new string[0];
        public string[][] FileNames = new string[0][];
        public Texture2D[][] Textures = new Texture2D[0][];

        private QueueType _currentType;
        private string _currentUrl;
        private string _currentOptions;

        public int currentIndex = 0;
        public int slideCount = 0;

        protected void AddSourceQueue(string url, string options)
        {
            ConsoleDebug($"[AddSourceQueue] {url}, {options}");
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.AddSourceLocal);
            dic.SetValue("url", url);
            dic.SetValue("options", options);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                return;
            }

            AddQueue(json.String);
        }

        protected void RemoveSourceQueue(string url)
        {
            ConsoleDebug($"[RemoveSourceQueue] {url}");
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.RemoveSource);
            dic.SetValue("url", url);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                return;
            }

            AddSyncQueue(json.String);
        }

        private void AddQueue(string queue)
        {
            ConsoleDebug($"[AddQueue] {queue}, isProcessing: {_isProcessing}");
            if (queue.IsNullOrEmpty()) return;
            _queue = _queue.Append(queue);
            if (_isProcessing) return;

            _isProcessing = true;
            ProcessQueue();
        }

        protected void SeekTo(int index)
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.SeekTo);
            dic.SetValue("index", index);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                return;
            }
            
            AddSyncQueue(json.String);
        }

        private void ProcessQueue()
        {
            if (_queue.Length == 0)
            {
                _isProcessing = false;
                HideSyncingModal();
                return;
            }

            _queue = _queue.__Shift(out var queue);
            if (!VRCJson.TryDeserializeFromJson(queue, out var data)) return;
            var type = Utils.ParseQueue(data);
            _currentType = type;
            ConsoleDebug($"Processing Queue: {queue}, Type: {type}");
            switch (type)
            {
                case QueueType.AddSourceLocal:
                    AddSourceLocal(data);
                    break;
                case QueueType.AddSource:
                    AddSource(data);
                    break;
                case QueueType.RemoveSource:
                    RemoveSource(data);
                    break;
                case QueueType.SeekTo:
                    Seek(data);
                    break;
                case QueueType.SyncAll:
                    SyncAll(data);
                    break;
                case QueueType.UpdateList:
                    UpdateList(data);
                    break;
                case QueueType.RequestSyncAll:
                    DoSyncAll();
                    break;
                case QueueType.None:
                default:
                    break;
            }
        }

        private void AddSourceLocal(DataToken data)
        {
            ConsoleDebug($"[AddSourceLocal] {data}");
            if (!data.DataDictionary.TryGetValue("url", out var url) ||
                !data.DataDictionary.TryGetValue("options", out var options)) return;
            options.String.ParseSourceOptions(out var type);
            _currentUrl = url.String;
            _currentOptions = options.String;
            ShowSyncingModal($"Loading {_currentUrl}");
            LLIFetchImage(_currentUrl, type, _currentOptions);
        }

        private void AddSource(DataToken data)
        {
            ConsoleDebug($"[AddSource] {data}");
            if (!data.DataDictionary.TryGetValue("url", out var url) ||
                !data.DataDictionary.TryGetValue("options", out var options)) return;
            options.String.ParseSourceOptions(out var type);
            _currentUrl = url.String;
            _currentOptions = options.String;
            ShowSyncingModal($"Loading {_currentUrl}");
            LLIFetchImage(_currentUrl, type, _currentOptions);
        }

        private void RemoveSource(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("url", out var url)) return;
            var source = url.String;
            if (!Sources.Has(source, out var index)) return;

            var removeCount = FileNames[index].Length;

            Sources = Sources.Remove(index, out var sourceUrl);
            Options = Options.Remove(index);
            FileNames = FileNames.Remove(index, out var removeFileNames);
            Textures = Textures.Remove(index);
            for (int i = 0; i < removeFileNames.Length; i++)
            {
                controller.CcReleaseTexture(sourceUrl, removeFileNames[i]);
            }
            if (currentIndex >= slideCount - removeCount && Networking.IsOwner(gameObject))
            {
                SeekTo(slideCount - removeCount);
            }

            UrlsUpdated();
            ProcessQueue();
        }

        private void Seek(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("index", TokenType.Double,out var indexToken))
            {
                ProcessQueue();
                return;
            }
            var index = (int)indexToken.Double;
            if ((index < 0 || index >= slideCount) && index != 0)
            {
                ProcessQueue();
                return;
            }
            currentIndex = index;
            IndexUpdated(index);
            ProcessQueue();
        }

        private void SyncAll(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("sources", TokenType.DataList,out var sources) ||
                !data.DataDictionary.TryGetValue("options", TokenType.DataList, out var options) ||
                !data.DataDictionary.TryGetValue("index", TokenType.Double, out var indexToken) ||
                sources.DataList.Count != options.DataList.Count)
            {
                ConsoleError($"[SyncAll] Invalid data: {data}");
                ProcessQueue();
                return;
            }
            var newSources = sources.DataList.ToStringArray();
            var newOptions = options.DataList.ToStringArray();

            Sources.Diff(newSources, out var toUnloadSources, out var toLoadSources);
            Options.Diff(newOptions, out var toUnloadOptions, out var toLoadOptions);

            var toUnload = toUnloadSources.Merge(toUnloadOptions).IntUnique();
            var toLoad = toLoadSources.Merge(toLoadOptions).IntUnique();

            foreach (var index in toUnload)
            {
                if (index < 0 || index >= Sources.Length) continue;
                Sources = Sources.Remove(index, out var source);
                Options = Options.Remove(index);
                FileNames = FileNames.Remove(index);
                controller.UnloadFilesFromUrl((IControlledDevice)this, source);
            }

            foreach (var index in toLoad)
            {
                ConsoleDebug($"{index}");
                if (index < 0 || index >= newSources.Length) continue;
                var dic = new DataDictionary();
                dic.SetValue("type", (int)QueueType.AddSource);
                dic.SetValue("url", newSources[index]);
                dic.SetValue("options", newOptions[index]);
                if (VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
                {
                    AddQueue(json.String);
                }
            }

            data.DataDictionary.SetValue("type", (int)QueueType.UpdateList);
            if (VRCJson.TrySerializeToJson(data.DataDictionary, JsonExportType.Minify, out var json1))
            {
                AddQueue(json1.String);
            }
            
            data.DataDictionary.SetValue("type", (int)QueueType.SeekTo);
            data.DataDictionary.SetValue("index", indexToken);
            if (VRCJson.TrySerializeToJson(data.DataDictionary, JsonExportType.Minify, out var json2))
            {
                AddQueue(json2.String);
            }

            UrlsUpdated();
            ProcessQueue();
        }

        private void DoSyncAll()
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.SyncAll);
            var sourceDic = new DataList();
            var optionDic = new DataList();
            for (int i = 0; i < Sources.Length; i++)
            {
                sourceDic.Add(Sources[i]);
                optionDic.Add(Options[i]);
            }

            dic.SetValue("sources", sourceDic);
            dic.SetValue("options", optionDic);
            dic.SetValue("index", currentIndex);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                return;
            }

            AddSyncQueue(json.String);
            ProcessQueue();
        }

        private void UpdateList(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("sources", out var sources) ||
                !data.DataDictionary.TryGetValue("options", out var options) ||
                sources.DataList.Count != options.DataList.Count) return;
            Sources = sources.DataList.ToStringArray();
            Options = options.DataList.ToStringArray();
            UrlsUpdated();
            ProcessQueue();
        }

        private void Abort()
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RequestReSyncAll));
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            RequestReSyncAll();
        }

        public void RequestReSyncAll()
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.RequestSyncAll);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                return;
            }

            AddQueue(json.String);
        }

        public override void _OnDeserialization()
        {
            base._OnDeserialization();
            ConsoleDebug($"[OnDeserialization] {SyncQueue}");
            if (SyncQueue.IsNullOrEmpty()) return;
            AddQueue(SyncQueue);
        }

        public override void OnFilesLoadSuccess(string source, string[] fileNames)
        {
            base.OnFilesLoadSuccess(source, fileNames);
            ShowSyncingModal($"Loaded {source}");
            ConsoleDebug($"[OnFilesLoadSuccess] {source} current: {_currentUrl}, currentType: {_currentType}");
            if (source != _currentUrl) return;
            if (_currentType == QueueType.AddSourceLocal)
            {
                ConsoleDebug($"[OnFilesLoadSuccess] AddSourceLocal: {_currentUrl}");
                var dic = new DataDictionary();
                dic.SetValue("type", (int)QueueType.AddSource);
                dic.SetValue("url", _currentUrl);
                dic.SetValue("options", _currentOptions);
                if (VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
                {
                    SyncQueue = json.String;
                    Sync();
                }
            }
            else if (_currentType == QueueType.AddSource)
            {
                ConsoleDebug($"[OnFilesLoadSuccess] AddSource: {_currentUrl}");
                Sources = Sources.Append(_currentUrl);
                Options = Options.Append(_currentOptions);
                FileNames = FileNames.Append(fileNames);
                var textures = new Texture2D[fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                {
                    textures[i] = controller.CcGetTexture(_currentUrl, fileNames[i]);
                }

                Textures = Textures.Append(textures);
                UrlsUpdated();
            }

            ProcessQueue();
        }

        public override void OnFileLoadProgress(string source, float progress)
        {
            base.OnFileLoadProgress(source, progress);
            ShowSyncingModal($"Loading {source} {progress:P}");
        }

        public override void OnFilesLoadFailed(LoadError error)
        {
            base.OnFilesLoadFailed(error);
            HideSyncingModal();
            error.ParseMessage(out var title, out var description);
            ShowErrorModal(title, description);
            ProcessQueue();
        }

        protected virtual void UrlsUpdated()
        {
            slideCount = 0;
            foreach (var fileNames in FileNames)
            {
                slideCount += fileNames.Length;
            }
        }

        protected virtual void IndexUpdated(int index)
        {
        }
    }
}