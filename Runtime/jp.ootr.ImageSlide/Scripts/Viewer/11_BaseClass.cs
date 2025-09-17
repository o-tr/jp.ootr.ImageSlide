using JetBrains.Annotations;
using jp.ootr.ImageDeviceController.CommonDevice;
using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public class BaseClass : CommonDevice
    {
        [SerializeField] internal bool isObjectSyncEnabled;
        [SerializeField] internal GameObject rootGameObject;
        protected readonly int AnimatorFollowMaster = Animator.StringToHash("FollowMaster");
        protected readonly int AnimatorIsLoading = Animator.StringToHash("IsLoading");
        
        // サムネイル読み込み表示用（UIThumbnailsで使用）
        protected GameObject[] _thumbnailListLoadingSpinners = new GameObject[0];

        public virtual void UrlsUpdated()
        {
        }

        public virtual void IndexUpdated(int index)
        {
        }
        
        protected virtual void LocalIndexUpdated(int index){}

        public virtual void InitImageSlide()
        {
        }

        public virtual void ShowSyncingModal([CanBeNull] string content)
        {
        }

        public virtual void HideSyncingModal()
        {
        }

        public virtual void SeekModeChanged(SeekMode seekMode)
        {
        }

        // 読み込み表示解除漏れ対策：すべての読み込み表示を強制的に非表示にする
        protected void ForceHideAllLoadingStates()
        {
            // メインスライドの読み込み表示を非表示
            animator.SetBool(AnimatorIsLoading, false);
            
            // サムネイルの読み込み表示も非表示にする
            for (int i = 0; i < _thumbnailListLoadingSpinners.Length; i++)
            {
                _thumbnailListLoadingSpinners[i].SetActive(false);
            }
        }
    }
}
