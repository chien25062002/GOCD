using System;
using UnityEngine;

namespace GOCD.Framework
{
    [Serializable]
    public class GOCDValue<T>
    {
        [SerializeField] T _value;
        
        public event Action<T> OnBeforeChanged;
        public event Action<T> OnValueChanged;

        public T value
        {
            get => _value;
            set
            {
                if (!_value.Equals(value))
                {
                    OnBeforeChanged?.Invoke(_value);
                    _value = value;
                    OnValueChanged?.Invoke(_value);
                }
            }
        }

        public GOCDValue(T defaultValue)
        {
            _value = defaultValue;
        }
    }
}
