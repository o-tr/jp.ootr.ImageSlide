using System;
using jp.ootr.common;
using UnityEngine;

namespace jp.ootr.ImageSlide.Viewer
{
    public enum SeekMode
    {
        AllowAll,
        AllowPreviousOnly,
        DisallowAll
    }
    public class UISeekMode : BaseClass
    {
        [SerializeField] internal SeekMode seekMode;
        [SerializeField] private RectTransform presentationTransform;
        [SerializeField] private GameObject mainView;
        [SerializeField] private GameObject slideList;

        protected virtual void OnEnable()
        {
            slideList.SetActive(seekMode != SeekMode.DisallowAll);
            presentationTransform.ToFillChildrenVertical(4);
        }
    }
}
