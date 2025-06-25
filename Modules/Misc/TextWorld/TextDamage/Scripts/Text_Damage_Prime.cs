using GOCD.Framework.Audio;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GOCD.Framework.Module.TextDamage
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Text_Damage_Prime : UI_TextBase_Floating, ITextDamage
    {
        [SerializeField] Image _imgCritical;

        [Title("Config")] 
        [LabelText("Sprite Critical")] 
        [PreviewField(50, ObjectFieldAlignment.Left)] 
        [SerializeField] Sprite _sprCritical;

        [Space] 
        [FoldoutGroup("Colors", false)] 
        [SerializeField] Color _normalColor = new(1.0f, 0.4f, 0.4f, 1f);

        [FoldoutGroup("Colors")]
        [SerializeField] Color _criticalColor = new(1.0f, 0.84f, 0.0f);

        [Space] [SerializeField] float _liveTime = 3f;

        [SerializeField] PoolPrefabConfig _poolConfig;
        [SerializeField] AudioConfig _sfxGetHit;

        [Title("Animations")]
        [ListDrawerSettings(ListElementLabelName = "displayName", AddCopiesLastElement = false)]
        [SerializeReference]
        TextAnimation[] _animations;

        Sequence _sequence;

        public override float liveTime => _liveTime;

        public void Show(int damage, Vector3 position, bool isCrit)
        {
            RectTransformCached.position = position;
            
            // Set data
            var msg = $"{UtilsNumber.Format(damage)}";
            text.text = msg;
            text.color = isCrit ? _criticalColor : _normalColor;
            _imgCritical.enabled = isCrit;
            _imgCritical.sprite = _sprCritical;

            // Play sound
            if (_sfxGetHit != null) AudioManager.Play(_sfxGetHit);

            // Build or restart sequence
            PlayAnimations();
        }

        void PlayAnimations()
        {
            // Clear any existing sequence
            _sequence.Stop();

            _sequence = Sequence.Create(useUnscaledTime: true);

            if (!_animations.IsNullOrEmpty())
            {
                for (int i = 0; i < _animations.Length; i++)
                {
                    _animations[i].ApplyPrime(this, _sequence);
                }
            }
            else
            {
                _sequence.ChainDelay(liveTime);
            }

            _sequence.OnComplete(OnComplete, false);
        }

        void OnComplete()
        {
            PoolPrefabGlobal.Release(_poolConfig, gameObject);
        }
    }
}