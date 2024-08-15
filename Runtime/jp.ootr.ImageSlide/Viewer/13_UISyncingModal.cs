using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UISyncingModal : UISlide
    {
        [SerializeField] private GameObject syncingModal;
        [SerializeField] private Transform syncingModalContainerTransform;
        [SerializeField] private TextMeshProUGUI syncingModalContent;
        [SerializeField] private ContentSizeFitter syncingModalContentSizeFitter;

        public override void ShowSyncingModal(string content)
        {
            syncingModal.SetActive(true);
            syncingModalContent.text = content;
            syncingModalContentSizeFitter.SetLayoutVertical();
            syncingModalContainerTransform.ToListChildrenVertical(24, 24, true);
        }

        public override void HideSyncingModal()
        {
            syncingModal.SetActive(false);
        }
    }
}