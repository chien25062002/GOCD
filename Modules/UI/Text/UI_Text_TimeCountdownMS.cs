using UnityEngine;

namespace GOCD.Framework
{
    public class UI_Text_TimeCountdownMS : UI_Text_TimeCountdown
    {
        static readonly string s_format = "{0:00}:{1:00}";

        [SerializeField] string _textFormat;

        protected override void UpdateTimeDisplay(float timeToDisplay)
        {
            base.UpdateTimeDisplay(timeToDisplay);

            int time = Mathf.CeilToInt(timeToDisplay);

            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            if (string.IsNullOrEmpty(_textFormat))
                text.text = string.Format(s_format, minutes, seconds);
            else
                text.text = string.Format(_textFormat, minutes, seconds);
        }
    }
}
