using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework
{
    public class CameraPerspectiveFitView : MonoBehaviour
    {
        internal enum CallMethod
        {
            Awake = 0,
            Start = 1,
            OnEnable = 2,
            None = 3
        }

        [SerializeField] CallMethod _callMethod;
        [SerializeField] Vector2 _viewSize;
        [SerializeField] float _distance;
        
        Camera _camera;

        Camera camera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponent<Camera>();
                if (_camera == null)
                    _camera = GetComponentInChildren<Camera>();
                if (_camera == null)
                    _camera = Camera.main;
                return _camera;
            }
        }

        void Awake()
        {
            if (_callMethod == CallMethod.Awake)
            {
                FitView();
            }
        }

        void Start()
        {
            if (_callMethod == CallMethod.Start)
            {
                FitView();
            }
        }

        void OnEnable()
        {
            if (_callMethod == CallMethod.OnEnable)
            {
                FitView();
            }
        }

        void FitView()
        {
            if (camera == null)
            {
#if UNITY_EDITOR
                throw new Exception("Main camera doesn't exist on this scene.");
#endif
                return;
            }

            float fov = camera.FOVFitRectAtDistance(_viewSize, _distance);
            camera.fieldOfView = fov;
        }

        public Tween FitView(Vector2 viewSize, float distance, float duration, Ease ease, Action onStart = null, Action onComplete = null)
        {
            if (camera == null)
            {
#if UNITY_EDITOR
                throw new Exception("Main camera doesn't exist on this scene.");
#endif
                return null;
            }
            
            float fov = camera.FOVFitRectAtDistance(_viewSize, _distance);

            Tween tween = camera.DOFieldOfView(fov, duration).SetEase(ease).OnStart(() => onStart?.Invoke())
                .OnComplete(() => onComplete?.Invoke());
            return tween;
        }
        
#if UNITY_EDITOR
        [Button]
        void SaveThisView()
        {
            Plane plane = new Plane(Vector3.up, Vector2.zero);
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

            if (plane.Raycast(ray, out var length))
            {
                Vector3 point = ray.GetPoint(length);

                Vector3 localPoint = camera.transform.InverseTransformPoint(point);
                _distance = Vector3.Distance(localPoint, camera.transform.position);

                _viewSize = camera.SizeAtDistance(_distance);
            }
        }
#endif
    }
}
