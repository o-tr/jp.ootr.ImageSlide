﻿using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace jp.ootr.ImageSlide
{
    public class LogicQueue : LogicSync
    {
        public int currentIndex;

        public int slideCount;

        private readonly string[] _logicQueuePrefix = { "LogicQueue" };
        private string _currentOptions;

        private QueueType _currentType;
        private string _currentUrl;
        private string[][] _fileNames = new string[0][];
        private bool _isProcessing;
        private string[] _queue = new string[0];
        protected string[] Options = new string[0];

        protected string[] Sources = new string[0];
        public Texture2D[][] Textures = new Texture2D[0][];

        public string[][] FileNames
        {
            get => _fileNames;
            set
            {
                _fileNames = value;
                var count = 0;
                foreach (var fileNames in _fileNames)
                    count += fileNames.Length;
                slideCount = count;
            }
        }

        protected void AddSourceQueue(string url, string options)
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.AddSourceLocal);
            dic.SetValue("url", url);
            dic.SetValue("options", options);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                ConsoleError($"failed to serialize add source json: {json}, {url}, {options}", _logicQueuePrefix);
                return;
            }

            ConsoleDebug($"add source to queue: {url}, {options}", _logicQueuePrefix);
            AddQueue(json.String);
        }

        protected void RemoveSourceQueue(string url)
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.RemoveSource);
            dic.SetValue("url", url);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                ConsoleError($"failed to serialize remove source json: {json}, {url}", _logicQueuePrefix);
                return;
            }

            ConsoleDebug($"remove source from queue: {url}", _logicQueuePrefix);
            AddSyncQueue(json.String);
        }

        private void AddQueue(string queue)
        {
            if (queue.IsNullOrEmpty())
            {
                ConsoleWarn("failed to add queue due to empty queue", _logicQueuePrefix);
                return;
            }

            _queue = _queue.Append(queue);
            ConsoleDebug($"add queue: {queue}", _logicQueuePrefix);
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
                ConsoleError($"failed to serialize seek to json: {json}, {index}", _logicQueuePrefix);
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
                ConsoleDebug("Queue is empty", _logicQueuePrefix);
                return;
            }

            _queue = _queue.__Shift(out var queue);
            if (!VRCJson.TryDeserializeFromJson(queue, out var data))
            {
                ConsoleError($"failed to deserialize queue: {queue}", _logicQueuePrefix);
                return;
            }

            var type = Utils.ParseQueue(data);
            _currentType = type;
            ConsoleDebug($"process from queue: {type}, {queue}", _logicQueuePrefix);
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
            if (!data.DataDictionary.TryGetValue("url", out var url) ||
                !data.DataDictionary.TryGetValue("options", out var options))
            {
                ConsoleError($"url or options not found in local source: {data}", _logicQueuePrefix);
                return;
            }

            options.String.ParseSourceOptions(out var type);
            _currentUrl = url.String;
            _currentOptions = options.String;
            ShowSyncingModal($"Loading {_currentUrl}");
            ConsoleDebug($"load local source: {_currentUrl}, {type}, {options}", _logicQueuePrefix);
            LLIFetchImage(_currentUrl, type, _currentOptions);
        }

        private void AddSource(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("url", out var url) ||
                !data.DataDictionary.TryGetValue("options", out var options))
            {
                ConsoleError($"url or options not found in source: {data}", _logicQueuePrefix);
                return;
            }

            options.String.ParseSourceOptions(out var type);
            _currentUrl = url.String;
            _currentOptions = options.String;
            ShowSyncingModal($"Loading {_currentUrl}");
            ConsoleDebug($"load source: {_currentUrl}, {type}, {options}", _logicQueuePrefix);
            LLIFetchImage(_currentUrl, type, _currentOptions);
        }

        private void RemoveSource(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("url", out var url))
            {
                ConsoleError($"url not found in remove source: {data}", _logicQueuePrefix);
                return;
            }

            var source = url.String;
            if (!Sources.Has(source, out var index))
            {
                ConsoleError($"source not found in current sources: {source}", _logicQueuePrefix);
                return;
            }

            Sources = Sources.Remove(index, out var sourceUrl);
            Options = Options.Remove(index);
            FileNames = FileNames.Remove(index, out var removeFileNames);
            Textures = Textures.Remove(index);
            for (var i = 0; i < removeFileNames.Length; i++) controller.CcReleaseTexture(sourceUrl, removeFileNames[i]);

            if (currentIndex >= slideCount && Networking.IsOwner(gameObject))
            {
                ConsoleDebug($"seek to last index: {slideCount - 1}", _logicQueuePrefix);
                SeekTo(slideCount - 1);
            }

            UrlsUpdated();
            ProcessQueue();
        }

        private void Seek(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("index", TokenType.Double, out var indexToken))
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
            if (!data.DataDictionary.TryGetValue("sources", TokenType.DataList, out var sources) ||
                !data.DataDictionary.TryGetValue("options", TokenType.DataList, out var options) ||
                !data.DataDictionary.TryGetValue("index", TokenType.Double, out var indexToken) ||
                sources.DataList.Count != options.DataList.Count)
            {
                ConsoleError($"sources or options not found in sync all: {data}", _logicQueuePrefix);
                ProcessQueue();
                return;
            }

            ConsoleDebug($"sync all: {sources}, {options}, {indexToken}", _logicQueuePrefix);

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
                controller.UnloadFilesFromUrl(this, source);
            }

            foreach (var index in toLoad)
            {
                if (index < 0 || index >= newSources.Length) continue;
                var dic = new DataDictionary();
                dic.SetValue("type", (int)QueueType.AddSource);
                dic.SetValue("url", newSources[index]);
                dic.SetValue("options", newOptions[index]);
                if (VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
                    AddQueue(json.String);
                else
                    ConsoleError(
                        $"failed to serialize add source json: {json}, {newSources[index]}, {newOptions[index]}",
                        _logicQueuePrefix);
            }

            data.DataDictionary.SetValue("type", (int)QueueType.UpdateList);
            if (VRCJson.TrySerializeToJson(data.DataDictionary, JsonExportType.Minify, out var json1))
                AddQueue(json1.String);
            else
                ConsoleError($"failed to serialize update list json: {json1}", _logicQueuePrefix);

            data.DataDictionary.SetValue("type", (int)QueueType.SeekTo);
            data.DataDictionary.SetValue("index", indexToken);
            if (VRCJson.TrySerializeToJson(data.DataDictionary, JsonExportType.Minify, out var json2))
                AddQueue(json2.String);
            else
                ConsoleError($"failed to serialize seek to json: {json2}", _logicQueuePrefix);

            UrlsUpdated();
            ProcessQueue();
        }

        private void DoSyncAll()
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.SyncAll);
            var sourceDic = new DataList();
            var optionDic = new DataList();
            for (var i = 0; i < Sources.Length; i++)
            {
                sourceDic.Add(Sources[i]);
                optionDic.Add(Options[i]);
            }

            dic.SetValue("sources", sourceDic);
            dic.SetValue("options", optionDic);
            dic.SetValue("index", currentIndex);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                ConsoleError($"failed to serialize sync all json: {json}", _logicQueuePrefix);
                return;
            }

            AddSyncQueue(json.String);
            ProcessQueue();
        }

        private void UpdateList(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("sources", out var sources) ||
                !data.DataDictionary.TryGetValue("options", out var options) ||
                sources.DataList.Count != options.DataList.Count)
            {
                ConsoleError($"sources or options not found in update list: {data}", _logicQueuePrefix);
                return;
            }

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
            ConsoleDebug("try to request resync all due to player joined", _logicQueuePrefix);
            RequestReSyncAll();
        }

        public void RequestReSyncAll()
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.RequestSyncAll);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                ConsoleError($"failed to serialize request sync all json: {json}", _logicQueuePrefix);
                return;
            }

            ConsoleDebug("request resync all", _logicQueuePrefix);
            AddQueue(json.String);
        }

        public override void _OnDeserialization()
        {
            base._OnDeserialization();
            if (SyncQueue.IsNullOrEmpty()) return;
            ConsoleDebug($"add sync queue from deserialization: {SyncQueue}", _logicQueuePrefix);
            AddQueue(SyncQueue);
        }

        public override void OnFilesLoadSuccess(string source, string[] fileNames)
        {
            base.OnFilesLoadSuccess(source, fileNames);
            ShowSyncingModal($"Loaded {source}");
            ConsoleDebug($"success to load files: {source}, {fileNames}", _logicQueuePrefix);
            if (source != _currentUrl) return;
            if (_currentType == QueueType.AddSourceLocal)
            {
                ConsoleDebug($"send add source to other clients: {_currentUrl}", _logicQueuePrefix);
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
                ConsoleDebug($"add source to current sources: {_currentUrl}", _logicQueuePrefix);
                Sources = Sources.Append(_currentUrl);
                Options = Options.Append(_currentOptions);
                FileNames = FileNames.Append(fileNames);
                var textures = new Texture2D[fileNames.Length];
                for (var i = 0; i < fileNames.Length; i++)
                    textures[i] = controller.CcGetTexture(_currentUrl, fileNames[i]);

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
            ConsoleWarn($"failed to load files: {title}, {description}", _logicQueuePrefix);
            ShowErrorModal(title, description);
            ProcessQueue();
        }

        protected virtual void UrlsUpdated()
        {
        }

        protected virtual void IndexUpdated(int index)
        {
        }
    }
}