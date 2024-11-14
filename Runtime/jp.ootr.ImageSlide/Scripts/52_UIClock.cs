using TMPro;
using UnityEngine;

namespace jp.ootr.ImageSlide
{
    public class UIClock : UIStopWatch {
        [SerializeField] private TextMeshProUGUI clockText;

        public override void InitController()
        {
            base.InitController();
            ClockTick();
        }

        public void ClockTick()
        {
            UpdateClockText();
            SendCustomEventDelayedSeconds(nameof(ClockTick), 0.25f);
        }
        
        private void UpdateClockText()
        {
            var time = System.DateTime.Now;
            clockText.text = $"{time.Hour:D2}:{time.Minute:D2}:{time.Second:D2}";
        }
    }
}
