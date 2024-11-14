using UnityEngine;
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

        public static Texture2D GetByIndex(this Texture2D[][] texturesList, int index)
        {
            return texturesList.GetByIndex(index, out var tmp1, out var tmp2);
        }

        public static Texture2D GetByIndex(this Texture2D[][] texturesList, int index, out int sourceIndex,
            out int fileIndex)
        {
            sourceIndex = -1;
            fileIndex = -1;
            for (var i = 0; i < texturesList.Length; i++)
            {
                var textures = texturesList[i];
                if (index < textures.Length)
                {
                    sourceIndex = i;
                    fileIndex = index;
                    return textures[index];
                }

                index -= textures.Length;
            }

            return null;
        }
    }

    public enum QueueType
    {
        None,
        AddSourceLocal,
        AddSource,
        RequestSyncAll,
        SyncAll,
        RemoveSource,
        SeekTo,
        UpdateList,
        UpdateSeekMode,
    }
}
