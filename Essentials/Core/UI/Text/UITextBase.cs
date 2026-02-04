using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

using CodeSketch.Mono;

namespace CodeSketch.Core.Text
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextBase : MonoBase
    {
        [SerializeField] TextMeshProUGUI _text;

        public TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                    _text = GetComponentInChildren<TextMeshProUGUI>();
                return _text;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureText()
        {
            if (_text == null) _text = Text;
        }

        protected virtual void Awake()
        {
            EnsureText();
        }

        protected virtual void OnValidate()
        {
            EnsureText();
        }
    }
}
