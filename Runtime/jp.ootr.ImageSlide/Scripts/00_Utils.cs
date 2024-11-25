using JetBrains.Annotations;
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

        [CanBeNull]
        public static Texture2D GetByIndex([CanBeNull] this Texture2D[][] texturesList, int index)
        {
            return texturesList.GetByIndex(index, out var void1, out var void2);
        }

        [CanBeNull]
        public static Texture2D GetByIndex([CanBeNull] this Texture2D[][] texturesList, int index, out int sourceIndex,
            out int fileIndex)
        {
            sourceIndex = -1;
            fileIndex = -1;
            if (texturesList == null) return null;
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
        
        public static bool GetByIndex([CanBeNull] this string[][] fileNames, int index, out int sourceIndex, out int fileIndex)
        {
            sourceIndex = -1;
            fileIndex = -1;
            if (fileNames == null) return false;
            for (var i = 0; i < fileNames.Length; i++)
            {
                var names = fileNames[i];
                if (index < names.Length)
                {
                    sourceIndex = i;
                    fileIndex = index;
                    return true;
                }

                index -= names.Length;
            }

            return false;
        }
    }

    public enum QueueType
    {
        None,
        AddSourceLocal, //1
        AddSource,//2
        SyncAll,//3
        RemoveSource,//4
        SeekTo,//5
        UpdateList,//6
        UpdateSeekMode,//7
        RequestSyncAll,//8
    }
}
