using System;
using jp.ootr.common;
using TMPro;
using UnityEngine;

namespace jp.ootr.ImageSlide
{
    public class UIStopWatch : UINextSlide
    {
        [SerializeField] private TextMeshProUGUI stopWatchText;

        private readonly int _animatorStopWatchState = Animator.StringToHash("StopWatchState");
        private bool _isStopWatchRunning;
        private ulong _stopWatchOffset;

        private ulong _stopWatchTime;

        public void StartStopWatch()
        {
            if (_isStopWatchRunning)
            {
                _isStopWatchRunning = false;
                return;
            }

            animator.SetInteger(_animatorStopWatchState, 1);
            _stopWatchTime = DateTime.Now.ToUnixTime() - _stopWatchOffset;
            _isStopWatchRunning = true;
            SendCustomEventDelayedSeconds(nameof(CountUpStopWatch), 0.1f);
        }

        public void ResetStopWatch()
        {
            _stopWatchTime = DateTime.Now.ToUnixTime();
            stopWatchText.text = "00:00:00";
            _isStopWatchRunning = false;
            _stopWatchOffset = 0;
            animator.SetInteger(_animatorStopWatchState, 0);
        }

        public void CountUpStopWatch()
        {
            if (!_isStopWatchRunning)
            {
                animator.SetInteger(_animatorStopWatchState, 2);
                _stopWatchOffset = DateTime.Now.ToUnixTime() - _stopWatchTime;
                return;
            }

            var time = TimeSpan.FromSeconds(DateTime.Now.ToUnixTime() - _stopWatchTime);
            stopWatchText.text = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            SendCustomEventDelayedSeconds(nameof(CountUpStopWatch), 0.1f);
        }
    }
}
