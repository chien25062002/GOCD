using GOCD.Framework.Data;
using UnityEngine;

namespace GOCD.Framework
{
    public class UIHidden : MonoCached
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        CanvasGroup _canvasGroup;

         void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = GameObjectCached.AddComponent<CanvasGroup>();

            DataMaster.UiHidden.OnValueChanged += UIHiddenValue_EventValueChanged;

            _canvasGroup.alpha = DataMaster.UiHidden.value ? 0.0f : 1.0f;
        }

         void OnDestroy()
        {
            DataMaster.UiHidden.OnValueChanged -= UIHiddenValue_EventValueChanged;
        }

         void UIHiddenValue_EventValueChanged(bool isHidden)
        {
            _canvasGroup.alpha = DataMaster.UiHidden.value ? 0.0f : 1.0f;
        }
#endif
    }
}
