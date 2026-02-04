using System;
using UnityEngine;

namespace CodeSketch.Data
{
#if CODESKETCH_MEMORYPACK
    [MemoryPack.MemoryPackable]
#else
    [System.Serializable]
#endif
    public partial class DataValue<T>
    {
#if CODESKETCH_MEMORYPACK
        [MemoryPack.MemoryPackInclude]
#else
        [SerializeField]
#endif        
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<T> OnValueChanged;

        public DataValue(T _value)
        {
            this._value = _value;
        }
    }
}