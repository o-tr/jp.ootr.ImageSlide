﻿using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide
{
    public class UIErrorModal : UIAnimationHandler
    {
        [SerializeField] private GameObject errorModal;
        [SerializeField] private RectTransform errorModalTransform;
        [SerializeField] private TextMeshProUGUI errorTitle;
        [SerializeField] private TextMeshProUGUI errorDescription;
        [SerializeField] private ContentSizeFitter errorDescriptionSizeFitter;

        protected virtual void ShowErrorModal(string title, string description)
        {
            errorModal.SetActive(true);
            errorTitle.text = title;
            errorDescription.text = description;
            errorDescriptionSizeFitter.SetLayoutVertical();

            errorModalTransform.ToListChildrenVertical(24, 24, true);
        }

        public virtual void CloseErrorModal()
        {
            errorModal.SetActive(false);
        }
    }
}
