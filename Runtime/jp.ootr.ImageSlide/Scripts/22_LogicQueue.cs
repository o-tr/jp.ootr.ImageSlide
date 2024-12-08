using JetBrains.Annotations;
using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using jp.ootr.ImageSlide.Viewer;
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
        
        private bool _isInitialized;

        public string[] GetSources()
        {
            return Sources;
        }

        protected void AddSourceQueue([CanBeNull] string url, [CanBeNull] string options)
        {
            if (!url.IsValidUrl() || !options.ParseSourceOptions())
            {
                ConsoleError($"invalid url: {url}", _logicQueuePrefix);
                return;
            }

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

        protected void RemoveSourceQueue([CanBeNull] string url)
        {
            if (!url.IsValidUrl())
            {
                ConsoleError($"invalid url: {url}", _logicQueuePrefix);
                return;
            }

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

        private void AddQueue([CanBeNull] string queue)
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

        private void AddHeadQueue([CanBeNull] string queue)
        {
            if (queue.IsNullOrEmpty())
            {
                ConsoleWarn("failed to add queue due to empty queue", _logicQueuePrefix);
                return;
            }

            _queue = _queue.Unshift(queue);
            ConsoleDebug($"add queue to head: {queue}", _logicQueuePrefix);
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

        protected void SyncSeekMode(SeekMode mode)
        {
            var modeInt = (int)mode;
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.UpdateSeekMode);
            dic.SetValue("mode", modeInt);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                ConsoleError($"failed to serialize update seek mode json: {json}, {mode}", _logicQueuePrefix);
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

            _queue = _queue.Shift(out var queue);
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
                case QueueType.UpdateSeekMode:
                    ApplySeekMode(data);
                    break;
                case QueueType.RequestSyncAll:
                    DoSyncAll();
                    break;
                case QueueType.None:
                    break;
                default:
                    ConsoleError($"unknown queue type: {type}", _logicQueuePrefix);
                    break;
            }
        }

        private void AddSourceLocal(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("url", out var url) ||
                !data.DataDictionary.TryGetValue("options", out var options))
            {
                ConsoleError($"url or options not found in local source: {data}", _logicQueuePrefix);
                ProcessQueue();
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
                ProcessQueue();
                return;
            }

            if (Networking.IsOwner(gameObject))
            {
                ConsoleDebug($"ignore add source because owner", _logicQueuePrefix);
                ProcessQueue();
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
                ProcessQueue();
                return;
            }

            var source = url.String;
            if (!Sources.Has(source, out var index))
            {
                ConsoleError($"source not found in current sources: {source}", _logicQueuePrefix);
                ProcessQueue();
                return;
            }

            Sources = Sources.Remove(index);
            Options = Options.Remove(index);
            FileNames = FileNames.Remove(index);
            
            if (currentIndex >= slideCount && Networking.IsOwner(gameObject))
            {
                if (slideCount == 0)
                {
                    ConsoleDebug("seek to 0 due to no slide", _logicQueuePrefix);
                    SeekTo(0);
                }
                else
                {
                    ConsoleDebug($"seek to last index: {slideCount - 1}", _logicQueuePrefix);
                    SeekTo(slideCount - 1);
                }
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
            if (index < 0 || (index >= slideCount && index != 0))
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
            if (Networking.IsOwner(gameObject))
            {
                ConsoleDebug($"ignore sync all because owner", _logicQueuePrefix);
                ProcessQueue();
                return;
            }
            if (!data.DataDictionary.TryGetValue("sources", TokenType.DataList, out var sources) ||
                !data.DataDictionary.TryGetValue("options", TokenType.DataList, out var options) ||
                !data.DataDictionary.TryGetValue("index", TokenType.Double, out var indexToken) ||
                sources.DataList.Count != options.DataList.Count)
            {
                ConsoleError($"sources or options not found in sync all: {data}", _logicQueuePrefix);
                ProcessQueue();
                return;
            }
            _isInitialized = true;

            ConsoleDebug($"sync all: {sources}, {options}, {indexToken}", _logicQueuePrefix);

            var newSources = sources.DataList.ToStringArray();
            var newOptions = options.DataList.ToStringArray();

            Sources.Diff(newSources, out var toUnloadSources, out var toLoadSources);
            Options.Diff(newOptions, out var toUnloadOptions, out var toLoadOptions);

            var toUnload = toUnloadSources.Merge(toUnloadOptions).Unique();
            var toLoad = toLoadSources.Merge(toLoadOptions).Unique();

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

            ProcessQueue();
        }

        public void OnSyncAllRequested()
        {
            if (!Networking.IsOwner(gameObject))
            {
                ConsoleDebug("send sync all request to owner", _logicQueuePrefix);
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(OnSyncAllRequested));
            }
            else
            {
                ConsoleDebug("do sync all", _logicQueuePrefix);
                RequestSyncAll();
            }
        }
        
        private void RequestSyncAll()
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.RequestSyncAll);
            if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
            {
                ConsoleError($"failed to serialize request sync all json: {json}", _logicQueuePrefix);
                return;
            }
            if (_queue.Has(json.String))
            {
                ConsoleDebug("skip request sync all because already pending", _logicQueuePrefix);
                return;
            }

            AddQueue(json.String);
            if (_isProcessing) return;
            _isProcessing = true;
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
                ProcessQueue();
                return;
            }

            AddSyncQueue(json.String);
            ProcessQueue();
        }

        private void ApplySeekMode(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("mode", TokenType.Double, out var modeToken))
            {
                ProcessQueue();
                return;
            }

            var mode = (int)modeToken.Double;

            SeekModeChanged((SeekMode)mode);
            ProcessQueue();
        }

        protected virtual void SeekModeChanged(SeekMode mode)
        {
        }

        private void UpdateList(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("sources", out var sources) ||
                !data.DataDictionary.TryGetValue("options", out var options) ||
                sources.DataList.Count != options.DataList.Count)
            {
                ConsoleError($"sources or options not found in update list: {data}", _logicQueuePrefix);
                ProcessQueue();
                return;
            }
            
            Sources = sources.DataList.ToStringArray();
            Options = options.DataList.ToStringArray();
            var fileNames = new string[Sources.Length][];
            var error = false;

            for (int i = 0; i < Sources.Length; i++)
            {
                var files =  controller.CcGetFileNames(Sources[i]);
                if (files == null)
                {
                    Sources = Sources.Remove(i);
                    Options = Options.Remove(i);
                    fileNames = fileNames.Remove(i);
                    error = true;
                    continue;
                }
                fileNames[i] = files;
            }
            FileNames = fileNames;
            
            if (error)
            {
                ConsoleError($"failed to update list: {data}", _logicQueuePrefix);
                ProcessQueue();
                return;
            }
            
            UrlsUpdated();
            ProcessQueue();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            base.OnPlayerJoined(player);
            if (!player.isLocal) return;
            if (Networking.IsOwner(gameObject))
            {
                ConsoleDebug("skip initialization sync because owner", _logicQueuePrefix);
                _isInitialized = true;
                return;
            }
            RequestInitializationSync();
        }

        public void OnResyncClicked()
        {
            if (Networking.IsOwner(gameObject))
            {
                ConsoleDebug("skip resync because owner", _logicQueuePrefix);
                return;
            }
            _isInitialized = false;
            RequestInitializationSync();
        }

        public void RequestInitializationSync()
        {
            if (Networking.IsOwner(gameObject))
            {
                ConsoleDebug("skip initialization sync because owner", _logicQueuePrefix);
                _isInitialized = true;
                return;
            }
            if (_isInitialized) return;
            ConsoleDebug($"send sync all to owner", _logicQueuePrefix);
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(OnSyncAllRequested));
            SendCustomEventDelayedSeconds(nameof(RequestInitializationSync), 10);
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
            if (source != _currentUrl)
            {
                ConsoleInfo($"ignore loaded files: {source}, expected: {_currentUrl}", _logicQueuePrefix);
                return;
            }
            ShowSyncingModal($"Loaded {source}");
            ConsoleDebug($"success to load files: {source}, {fileNames}", _logicQueuePrefix);
            if (_currentType == QueueType.AddSourceLocal)
            {
                ConsoleDebug($"send add source to other clients: {_currentUrl}", _logicQueuePrefix);
                var dic = new DataDictionary();
                dic.SetValue("type", (int)QueueType.AddSource);
                dic.SetValue("url", _currentUrl);
                dic.SetValue("options", _currentOptions);
                if (VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
                {
                    AddSyncQueue(json.String);
                }
            }
            ConsoleDebug($"add source to current sources: {_currentUrl}", _logicQueuePrefix);
            Sources = Sources.Append(_currentUrl);
            Options = Options.Append(_currentOptions);
            FileNames = FileNames.Append(fileNames);

            UrlsUpdated();

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
