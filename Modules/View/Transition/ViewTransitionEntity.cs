using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework
{
    public class ViewTransitionEntity : MonoCached
    {
        [Title("Config")]
        [Range(0.1f, 1f)]
        [SerializeField] float _duration = 1f;

        [CustomValueDrawer("GUIDrawInsertTime")]
        [SerializeField] float _insertTime = 0f;

        [ListDrawerSettings(AddCopiesLastElement = true, ListElementLabelName = "displayName")]
        [SerializeReference] ViewTransition[] _transitions = new ViewTransition[0];

        RectTransform _rectTransform;

        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        public void Apply(View view)
        {
            for (int i = 0; i < _transitions.Length; i++)
            {
                Tween tween = _transitions[i].GetTween(this, _duration);

                if (_insertTime > 0f)
                    view.sequence.Insert(_insertTime, tween);
                else
                    view.sequence.Join(tween);
            }
        }

#if UNITY_EDITOR

        float GUIDrawInsertTime(float insertTime, GUIContent label)
        {
            return UnityEditor.EditorGUILayout.Slider(label, insertTime, 0f, 1.0f - _duration);
        }

#endif
    }
}
