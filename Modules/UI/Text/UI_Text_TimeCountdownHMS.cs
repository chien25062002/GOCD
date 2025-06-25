using UnityEngine;

namespace GOCD.Framework
{
    public class UI_Text_TimeCountdownHMS : UI_Text_TimeCountdown
    {
        [SerializeField] string _textFormat;

        static readonly string s_format = "{0:00}:{1:00}:{2:00}";

        protected override void UpdateTimeDisplay(float timeToDisplay)
        {
            base.UpdateTimeDisplay(timeToDisplay);

            if (timeToDisplay == 0f)
            {
                text.text = string.Format(s_format, 0, 0, 0);
                return;
            }

            timeToDisplay += 1;

            int hours = Mathf.FloorToInt(timeToDisplay / 3600f);
            int minutes = Mathf.FloorToInt((timeToDisplay - hours * 3600f) / 60f);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60f);

            if (!string.IsNullOrEmpty(_textFormat))
                text.text = string.Format(_textFormat, hours, minutes, seconds);
            else
                text.text = string.Format(s_format, hours, minutes, seconds);
        }
    }
}
