﻿using JetBrains.Annotations;
using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UISyncingModal : UIErrorModal
    {
        [SerializeField] private GameObject syncingModal;
        [SerializeField] private Transform syncingModalContainerTransform;
        [SerializeField] private TextMeshProUGUI syncingModalContent;
        [SerializeField] private ContentSizeFitter syncingModalContentSizeFitter;

        protected virtual void ShowSyncingModal([CanBeNull] string content)
        {
            syncingModal.SetActive(true);
            syncingModalContent.text = content;
            syncingModalContentSizeFitter.SetLayoutVertical();
            syncingModalContainerTransform.ToListChildrenVertical(24, 24, true);
        }

        protected virtual void HideSyncingModal()
        {
            syncingModal.SetActive(false);
        }
    }
}
