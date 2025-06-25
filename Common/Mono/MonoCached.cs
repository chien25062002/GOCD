using UnityEngine;

namespace GOCD.Framework
{
    public class MonoCached : MonoBehaviour
    {
        GameObject _gameObject;
        Transform _transform;
        RectTransform _rectTransform;

        public Transform TransformCached
        {
            get
            {
                if (_transform == null)
                    _transform = transform;

                return _transform;
            }
        }

        public GameObject GameObjectCached
        {
            get
            {
                if (_gameObject == null)
                    _gameObject = gameObject;

                return _gameObject;
            }
        }
        
        public RectTransform RectTransformCached
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = transform as RectTransform;
                return _rectTransform;
            }
        }
    }
}
