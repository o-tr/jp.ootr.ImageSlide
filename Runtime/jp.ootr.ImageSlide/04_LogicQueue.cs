using jp.ootr.common;
using jp.ootr.ImageDeviceController;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;
using Newtonsoft.Json.Linq;

namespace jp.ootr.ImageSlide
{
    public class LogicQueue : LogicLoadImage
    {
        [UdonSynced] protected string SyncQueue = string.Empty;
        
        private string[] _queue = new string[0];
        private bool _isProcessing;
        
        protected string[] _sources = new string[0];
        protected string[] _options = new string[0];
        protected string[][] _fileNames = new string[0][];
        
        private QueueType _currentType;
        private string _currentUrl;
        private string _currentOptions;
        
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
            SyncQueue = json.String;
            Sync();
        }
        
        protected void AddQueue(string queue)
        {
            ConsoleDebug($"[AddQueue] {queue}");
            if (queue.IsNullOrEmpty()) return;
            _queue = _queue.Append(queue);
            if (_isProcessing) return;
            
            _isProcessing = true;
            ProcessQueue();
        }
        
        private void ProcessQueue()
        {
            if (_queue.Length == 0)
            {
                _isProcessing = false;
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
            animator.SetFloat(AnimatorProgress,0);
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
            animator.SetFloat(AnimatorProgress,0);
            LLIFetchImage(_currentUrl, type, _currentOptions);
        }
        
        private void RemoveSource(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("url", out var url)) return;
            var source = url.String;
            if (!_sources.Has(source, out var index)) return;
            
            _sources = _sources.Remove(index);
            _options = _options.Remove(index);
            _fileNames = _fileNames.Remove(index);
            UrlsUpdated();
            ProcessQueue();
        }
        
        private void Seek(DataToken data)
        {
            if(!data.DataDictionary.TryGetValue("index", out var index)) return;
            
        }
        
        private void ReSyncAll()
        {
            
        }

        private void Abort()
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(RequestReSyncAll));
        }
        
        public void RequestReSyncAll()
        {
            var dic = new DataDictionary();
            dic.SetValue("type", (int)QueueType.SyncAll);
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
            animator.SetFloat(AnimatorProgress,-1);
            ConsoleDebug($"[OnFilesLoadSuccess] {source} current: {_currentUrl}, currentType: {_currentType}");
            if (source != _currentUrl) return;
            if (_currentType == QueueType.AddSourceLocal)
            {
                ConsoleDebug($"[OnFilesLoadSuccess] AddSourceLocal: {_currentUrl}");
                var dic = new DataDictionary();
                dic.SetValue("type", (int)QueueType.AddSource);
                dic.SetValue("url", _currentUrl);
                dic.SetValue("options", _currentOptions);
                if (!VRCJson.TrySerializeToJson(dic, JsonExportType.Minify, out var json))
                {
                    return;
                }
                SyncQueue = json.String;
                Sync();
            }
            else if(_currentType == QueueType.AddSource)
            {
                ConsoleDebug($"[OnFilesLoadSuccess] AddSource: {_currentUrl}");
                _sources = _sources.Append(_currentUrl);
                _options = _options.Append(_currentOptions);
                _fileNames = _fileNames.Append(fileNames);
                UrlsUpdated();
            }
            ProcessQueue();
        }

        public override void OnFileLoadProgress(string source, float progress)
        {
            base.OnFileLoadProgress(source, progress);
            animator.SetFloat(AnimatorProgress,progress);
        }

        public override void OnFilesLoadFailed(LoadError error)
        {
            base.OnFilesLoadFailed(error);
            animator.SetFloat(AnimatorProgress,-1);
            error.ParseMessage(out var title, out var description);
            ShowErrorModal(title, description);
            ProcessQueue();
        }
        
        protected virtual void UrlsUpdated()
        {
        }
    }
}