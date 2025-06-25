using DG.Tweening;
using GOCD.Framework.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GOCD.Framework.Module.TextDamage
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TextDamage : UI_TextBase_Floating, ITextDamage
    {
        [SerializeField] Image _imgCritical;
        
        [Space]
        
        [Title("Config")] 
        [LabelText("Sprite Critical")]
        [PreviewField(50, ObjectFieldAlignment.Left)]
        [SerializeField] Sprite _sprCritical;

        [Space] 
        
        [FoldoutGroup(groupName: "Colors", expanded: false)]
        [SerializeField] Color _normalColor = new Color(1.0f, 0.4f, 0.4f, 1f);
        [FoldoutGroup(groupName: "Colors")]
        [SerializeField] Color _criticalColor = new Color(1.0f, 0.84f, 0.0f);

        [Space] 
        
        [SerializeField] float _liveTime = 3f;

        [SerializeField] PoolPrefabConfig _poolConfig;
        [SerializeField] AudioConfig _sfxGetHit;

        [Title("Animations")] 
        [ListDrawerSettings(ListElementLabelName = "displayName", AddCopiesLastElement = false)]
        [SerializeReference] TextAnimation[] _animations;

        Sequence _sequence;

        public override float liveTime => _liveTime;

        public void Show(int damage, Vector3 position, bool isCrit)
        {
            RectTransformCached.position = position;
            
            ConstructSequence();
            
            string msg = $"{UtilsNumber.Format(damage)}";
            text.text = msg;
            text.color = isCrit ? _criticalColor : _normalColor;
            
            _imgCritical.enabled = isCrit;
            _imgCritical.sprite = _sprCritical;
            
            _sequence.Restart();
            _sequence.PlayForward();
        }

        #region Function -> Private

        void ConstructSequence()
        {
            if (_sequence != null) return;
            
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            _sequence.AppendCallback(() =>
            {
                if (_sfxGetHit != null) AudioManager.Play(_sfxGetHit);
            });

            if (!_animations.IsNullOrEmpty())
            {
                for (int i = 0; i < _animations.Length; i++)
                {
                    _animations[i].Apply(this, _sequence);
                }
            }
            else
            {
                _sequence.AppendInterval(liveTime);
            }

            _sequence.OnComplete(OnComplete);
            _sequence.SetAutoKill(false);
        }

        #endregion

        void OnComplete()
        {
            // return to pool
            PoolPrefabGlobal.Release(_poolConfig, gameObject);
        }
    }
}
