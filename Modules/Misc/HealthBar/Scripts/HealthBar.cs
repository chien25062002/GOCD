using System;
using CodeSketch.Core.Extensions;
using CodeSketch.Data;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace CodeSketch.Modules.HealthSystem
{
    public class HealthBar : HealthBarBase
    {
        [Serializable]
        internal class HealthBarConfig
        {
            [Header("Only For Editor")]
            public Vector2 normalSizeDelta = new Vector2(15f, 2f);
            
            [Space]
            
            [Header("Animations")]
            public float delayToChangeHp = 0.1f;
            public float durationChangeRedHp = 0.1f;
            public float durationChangeYellowHp = 0.5f;

            [Space] 
            
            public float scaleUp = 1.2f;
            public float scaleDuration = 0.25f;
            public Ease scaleEase = Ease.Linear;

            [Space] 
            
            public bool manuallyInitialize = true;

            [ShowIf("@manuallyInitialize == false")]
            public float defaultHp = 100;
        }

        [SerializeField] Transform _root;
        [SerializeField] SpriteRenderer _barFrame;
        [SerializeField] SpriteRenderer _barYellow;
        [SerializeField] SpriteRenderer _barRed;
        [SerializeField] TextMeshPro _textHP;

        [Space] 
        
        [SerializeField] HealthBarConfig _config;

        DataValue<float> _maxHealth = new DataValue<float>(0);
        DataValue<float> _health = new DataValue<float>(0);
        
        Tween _tweenChangeRedHp;
        Tween _tweenChangeYellowHp;
        Tween _tweenScale;

        public event Action EventDie;
        
        public DataValue<float> MaxHealth => _maxHealth;
        public DataValue<float> Health => _health;

        void Awake()
        {
            InitTween();
        }

        protected override void Start()
        {
            base.Start();
            
            if (!_config.manuallyInitialize)
                Init(_config.defaultHp, _config.defaultHp);
        }

        void OnDestroy()
        {
            _tweenScale?.Kill();
            _tweenChangeYellowHp?.Kill();
            _tweenChangeRedHp?.Kill();
        }

        void InitTween()
        {
            if (_tweenScale != null) return;

            _tweenScale = _root.DOScale(Vector3.one * _config.scaleUp, _config.scaleDuration).SetEase(_config.scaleEase)
                .SetLoops(2, LoopType.Yoyo);
            _tweenScale.SetAutoKill(false);
            _tweenScale.Restart();
            _tweenScale.Pause();
        }

        public override void Init(float maxHp, float hp)
        {
            _maxHealth.Value = maxHp;
            _health.Value = hp;

            if (_textHP != null)
            {
                _textHP.text = $"{(int)_health.Value}";
            }
            GameObjectCached.SetActive(true);

            _barYellow.DOSize(_config.normalSizeDelta);
            _barRed.DOSize(_config.normalSizeDelta);
            
            TakeDamage(0, true);
        }

        public override void Hide()
        {
            GameObjectCached.SetActive(false);
        }

        public override void TakeDamage(float damage, bool immediately = false)
        {
            if (Health.Value <= 0) return;

            Health.Value -= damage;

            if (Health.Value <= 0)
            {
                Health.Value = 0;
                EventDie?.Invoke();
            }
            
            float ratio = _health.Value / _maxHealth.Value;
            
            Vector2 toSizeDelta = new Vector2(_config.normalSizeDelta.x * ratio, _config.normalSizeDelta.y);
            _tweenChangeRedHp?.Kill();
            _tweenChangeYellowHp?.Kill();
            _tweenChangeRedHp = _barRed.DOSize(toSizeDelta, _config.durationChangeRedHp);
            
            _tweenChangeYellowHp = DOVirtual.DelayedCall(_config.delayToChangeHp, () =>
            {
                _tweenChangeYellowHp = _barYellow.DOSize(toSizeDelta, _config.durationChangeYellowHp);
            });

            float time = _root.localScale.x / _config.scaleUp;
            _tweenScale.Goto(Mathf.Lerp(0, 0.5f, time) * _config.scaleDuration, true);
            
            if (_health.Value == 0)
            {
                gameObject.SetActive(false);
            }

            if (_textHP != null) _textHP.text = $"{(int)_health.Value}";

            if (immediately)
            {
                _tweenScale?.Complete();
                _tweenChangeRedHp?.Complete();
                _tweenChangeYellowHp?.Complete();

                _barYellow.DOSize(toSizeDelta, 0f);
            }
        }
        
#if UNITY_EDITOR
        [Button]
        void Generate(float framePadding = 1f)
        {
            _barFrame.size = _config.normalSizeDelta + Vector2.one * framePadding;
            _barFrame.transform.localPosition = Vector2.zero;
            
            _barYellow.size = _config.normalSizeDelta;
            _barRed.size = _config.normalSizeDelta;

            _barYellow.transform.localPosition =
                _barRed.transform.localPosition = new Vector3(-_barRed.size.x / 2f, 0f);

            UpdateLayer();
            
            void UpdateLayer()
            {
                _barYellow.sortingLayerID = _barFrame.sortingLayerID;
                _barYellow.sortingOrder = _barFrame.sortingOrder + 1;
                
                _barRed.sortingLayerID = _barFrame.sortingLayerID;
                _barRed.sortingOrder = _barFrame.sortingOrder + 2;
            }
        }

        [Button]
        void TestDamage(int damage = 5)
        {
            TakeDamage(damage);
        }
#endif
    }
}
