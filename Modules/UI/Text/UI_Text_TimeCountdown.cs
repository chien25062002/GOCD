using System;
using UnityEngine;

namespace GOCD.Framework
{
    public class UI_Text_TimeCountdown : UITextBase
    {
        [Header("Config")]
        [SerializeField] bool _unscaledTime = true;

        float _timeRemain;
        
        public event Action EventTimeUp;
        public float TimeRemain => _timeRemain;
        
        #region  MonoBehaviour

        protected override void Tick()
        {
            base.Tick();
            
            _timeRemain -= _unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_timeRemain <= 0.0f)
            {
                _timeRemain = 0.0f;

                SetEnabled(false);
                UpdateTimeDisplay(_timeRemain);
                EventTimeUp?.Invoke();
            }

            UpdateTimeDisplay(_timeRemain);
        }

        #endregion
        
        #region Public

        public void Construct(float timeLeft)
        {
            _timeRemain = timeLeft;

            UpdateTimeDisplay(_timeRemain);

            enabled = timeLeft > 0;
        }

        public void SetEnabled(bool isEnabled)
        {
            enabled = isEnabled;
        }

        #endregion

        protected virtual void UpdateTimeDisplay(float timeToDisplay)
        {

        }
    }
}
