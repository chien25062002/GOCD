using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeSketch.Modules.HealthSystem
{
    public class HealthBarUI : HealthBarBase
    {
        [Serializable]
        internal class HealthBarConfig
        {
            public Vector2 normalSize = new Vector2(150f, 20f);

            [Space]
            public float delayToChangeHp = 0.1f;
            public float durationChangeRedHp = 0.1f;
            public float durationChangeYellowHp = 0.5f;

            [Space] 
            
            public float scaleDuration = 0.25f;
            public Ease scaleEase = Ease.Linear;
        }

        [SerializeField] RectTransform _root;
        [SerializeField] Image _barRed;
        [SerializeField] Image _barYellow;
        [SerializeField] TextMeshProUGUI _textHP;
        [SerializeField] HealthBarConfig _config;
        [SerializeField] bool _useTextPercent = false;

        public event Action EventDie;

        float _maxHealth;
        float _health;

        Tween _tweenRed, _tweenYellow, _tweenScale;

        public override void Init(float maxHp, float hp)
        {
            _maxHealth = maxHp;
            _health = hp;

            UpdateBar(1f);
            _textHP.text = ((int)_health).ToString();
            gameObject.SetActive(true);

            TakeDamage(0, true);
        }

        public override void TakeDamage(float damage, bool immediately = false)
        {
            if (_health <= 0) return;

            _health -= damage;
            if (_health <= 0)
            {
                _health = 0;
                EventDie?.Invoke();
            }

            float ratio = _health / _maxHealth;
            UpdateBar(ratio);
            
            _textHP.text = ((int)_health).ToString();

            if (_useTextPercent) _textHP.text = $"{_health * 1f / _maxHealth * 100f}%";

            if (_health == 0) gameObject.SetActive(false);

            _tweenRed?.Kill();
            _tweenYellow?.Kill();

            _tweenRed = DOVirtual.Float(_barRed.fillAmount, ratio, _config.durationChangeRedHp, (val) =>
            {
                _barRed.fillAmount = val;
            }).SetEase(Ease.Linear);
            
            _tweenYellow = DOVirtual.DelayedCall(_config.delayToChangeHp, () =>
            {
                _tweenYellow = DOVirtual.Float(_barYellow.fillAmount, ratio, _config.durationChangeYellowHp, (val) =>
                {
                    _barYellow.fillAmount = val;
                }).SetEase(Ease.Linear);
            });

            if (immediately)
            {
                _tweenRed?.Complete();
                _tweenYellow?.Complete();
                _barYellow.fillAmount = ratio;
            }
        }

        void UpdateBar(float ratio)
        {
            _barRed.fillAmount = ratio;
            _barYellow.fillAmount = ratio;
        }

        public override void Hide() => gameObject.SetActive(false);
    }
}
