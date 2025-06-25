using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PrimeTween;
using Sirenix.OdinInspector;

namespace CFramework
{
    public class UIGraphicScale : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Title("Reference")]
        [SerializeField] Transform _tfTarget;

        [Title("Config")]
        [SerializeField] float _targetScalePercent = 0.9f; // ví dụ: 0.9 = 90%
        [Min(0.01f)]
        [SerializeField] float _duration = 0.1f;

        Tween _tween;
        bool _isDown = false;
        Vector3 _initialScale;

        Button _button;
        Button button => _button != null ? _button : (_button = GetComponent<Button>());

        void Awake()
        {
            if (_tfTarget == null)
                _tfTarget = transform;

            _initialScale = _tfTarget.localScale;
        }

        void OnEnable()
        {
            _tfTarget.localScale = _initialScale;
        }

        void OnDisable()
        {
            _tween.Stop();
            _tfTarget.localScale = _initialScale;
            _isDown = false;
        }

        void OnDestroy()
        {
            _tween.Stop();
        }

        void ScaleTo(float percent)
        {
            Vector3 targetScale = _initialScale * percent;
            _tween.Stop();
            _tween = Tween.Scale(_tfTarget, _tfTarget.localScale, targetScale, _duration, Ease.OutQuad);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            _isDown = true;
            ScaleTo(_targetScalePercent);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!_isDown) return;

            _isDown = false;
            ScaleTo(1f);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (_isDown)
            {
                _isDown = false;
                ScaleTo(1f);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_isDown)
            {
                ScaleTo(_targetScalePercent);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            // optional: add click sound/feedback here
        }
    }
}
