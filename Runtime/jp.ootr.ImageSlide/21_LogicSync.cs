using jp.ootr.common;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon.Common.Enums;

namespace jp.ootr.ImageSlide
{
    public class LogicSync : UISyncingModal 
    {
        [UdonSynced] protected string SyncQueue = string.Empty;
        private string _localSyncQueue = string.Empty;
        
        private string[] _syncQueueArray = new string[0];
        private bool _isSyncing = false;
        
        protected virtual void AddSyncQueue(string data)
        {
            _syncQueueArray = _syncQueueArray.Append(data);
            if (_isSyncing) return;
            _isSyncing = true;
            DoSyncQueue();
        }
        
        public void DoSyncQueue()
        {
            if (_syncQueueArray.Length == 0)
            {
                if (Networking.IsOwner(gameObject))
                {
                    SyncQueue = string.Empty;
                    Sync();
                }
                _isSyncing = false;
                return;
            }
            _syncQueueArray = _syncQueueArray.__Shift(out var data);
            SyncQueue = data;
            _localSyncQueue = data;
            Sync();
        }

        public override void _OnDeserialization()
        {
            base._OnDeserialization();
            if (SyncQueue == _localSyncQueue)
            {
                SendCustomEventDelayedFrames(nameof(DoSyncQueue), 1, EventTiming.LateUpdate);
            }
        }
    }
}