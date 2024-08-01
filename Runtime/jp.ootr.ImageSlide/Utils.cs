using VRC.SDK3.Data;

namespace jp.ootr.ImageSlide
{
    public static class Utils
    {
        public static QueueType ParseQueue(DataToken data)
        {
            if (!data.DataDictionary.TryGetValue("type", out var type)) return QueueType.None;
            return (QueueType)(int)type.Double;
        }
        
    }
    
    public enum QueueType
    {
        None,
        AddSourceLocal,
        AddSource,
        SyncAll,
        RemoveSource,
        SeekTo,
        RemoveUnusedFiles
    }
}