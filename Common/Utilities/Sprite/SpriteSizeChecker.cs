using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CFramework
{
    public class SpriteSizeChecker : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] List<Sprite> _sprites = new List<Sprite>();
        [SerializeField] List<Bounds> _bounds = new List<Bounds>();

        [Button]
        void Clean()
        {
            _sprites.Clear();
            _bounds.Clear();
        }
        
        [Button]
        void Check()
        {
            for (int i = 0; i < _sprites.Count; i++)
            {
                _bounds.Add(_sprites[i].bounds);
            }
        }
#endif
    }
}
