using jp.ootr.common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace jp.ootr.ImageSlide.Viewer
{
    public class UIErrorModal : UISplashScreen
    {
        [SerializeField] private GameObject errorModal;
        [SerializeField] private RectTransform errorModalTransform;
        [SerializeField] private TextMeshProUGUI errorTitle;
        [SerializeField] private TextMeshProUGUI errorDescription;
        [SerializeField] private ContentSizeFitter errorDescriptionSizeFitter;

        public void ShowErrorModal(string title, string message)
        {
            errorModal.SetActive(true);
            errorTitle.text = title;
            errorDescription.text = message;
            errorDescriptionSizeFitter.SetLayoutVertical();

            errorModalTransform.ToListChildrenVertical(24, 24, true);
        }

        public void CloseErrorModal()
        {
            errorModal.SetActive(false);
        }
    }
}
