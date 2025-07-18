// using UnityEngine;
//
// namespace CFramework
// {
//     public class LookAtCamera : MonoCached
//     {
//         [System.Serializable]
//         public enum Direction
//         {
//             XYZ,
//             Y,
//             XY,
//             YZ
//         }
//
//         [SerializeField] Direction _direction = Direction.XYZ;
//
//         Transform _trans;
//         Vector3 _dir;
//         Quaternion _rotation;
//
//         void Start()
//         {
//             if (Camera.main != null) _trans = Camera.main.transform;
//         }
//
//         void LateUpdate()
//         {
//             if (_trans == null) return;
//
//             _dir = _trans.position - transformCached.position;
//
//             switch (_direction)
//             {
//                 case Direction.Y:
//                     _dir.y = 0f;
//                     break;
//                 case Direction.XY:
//                     _dir.z = 0f;
//                     break;
//                 case Direction.YZ:
//                     _dir.x = 0f;
//                     break;
//             }
//
//             if (_dir.sqrMagnitude < 0.0001f)
//                 return;
//
//             _rotation = Quaternion.LookRotation(-_dir.normalized);
//
//             transformCached.rotation = _rotation;
//         }
//     }
// }

using UnityEngine;

namespace GOCD.Framework
{
    public class LookAtCamera : MonoCached
    {
        public enum Direction { XYZ, Y, XY, YZ }

        [SerializeField] Direction _direction = Direction.XYZ;

        public void LookAt(Transform cam)
        {
            Vector3 dir = cam.position - TransformCached.position;

            switch (_direction)
            {
                case Direction.Y:  dir.y = 0f; break;
                case Direction.XY: dir.z = 0f; break;
                case Direction.YZ: dir.x = 0f; break;
            }

            if (dir.sqrMagnitude < 0.0001f)
                return;

            TransformCached.rotation = Quaternion.LookRotation(-dir.normalized);
        }

        void OnEnable()  => LookAtCameraManager.Register(this);
        void OnDisable() => LookAtCameraManager.Unregister(this);
        void OnDestroy() => LookAtCameraManager.Unregister(this);
    }
}

