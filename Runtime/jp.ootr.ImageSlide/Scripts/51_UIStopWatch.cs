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
        private bool _isTickScheduled;
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
            if (_isTickScheduled) return;
            _isTickScheduled = true;
            SendCustomEventDelayedSeconds(nameof(CountUpStopWatch), 0.1f);
        }

        public void ResetStopWatch()
        {
            _stopWatchTime = DateTime.Now.ToUnixTime();
            if (stopWatchText != null) stopWatchText.text = "00:00:00";
            _isStopWatchRunning = false;
            _stopWatchOffset = 0;
            animator.SetInteger(_animatorStopWatchState, 0);
        }

        public void CountUpStopWatch()
        {
            _isTickScheduled = false;
            if (!_isStopWatchRunning)
            {
                // Reset 直後の pending tick はスキップ(state 0 == Idle)。
                // Stop(pause) の場合のみ state 2 に遷移して経過分を offset に退避する。
                if (animator.GetInteger(_animatorStopWatchState) == 0) return;
                animator.SetInteger(_animatorStopWatchState, 2);
                _stopWatchOffset = DateTime.Now.ToUnixTime() - _stopWatchTime;
                return;
            }

            var time = TimeSpan.FromSeconds(DateTime.Now.ToUnixTime() - _stopWatchTime);
            if (stopWatchText != null)
                stopWatchText.text = $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            _isTickScheduled = true;
            SendCustomEventDelayedSeconds(nameof(CountUpStopWatch), 0.1f);
        }
    }
}
