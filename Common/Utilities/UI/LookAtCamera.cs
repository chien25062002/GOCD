using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Script giúp object quay mặt về phía camera theo trục tùy chọn (XYZ, Y, XY, YZ).
    /// </summary>
    public class LookAtCamera : MonoCached
    {
        [System.Serializable]
        public enum Direction
        {
            XYZ, // Quay đầy đủ theo 3 trục
            Y,   // Quay theo XZ (bỏ Y)
            XY,  // Quay theo XY (bỏ Z)
            YZ   // Quay theo YZ (bỏ X)
        }

        [SerializeField]
        Direction _direction = Direction.XYZ;

        Transform _cameraTransform;
        Vector3 _dir;
        Quaternion _rotation;

        void Start()
        {
            // Gán transform của Camera chính (main)
            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        void LateUpdate()
        {
            if (_cameraTransform == null)
                return;

            // Tính vector hướng từ object đến camera
            _dir = _cameraTransform.position - TransformCached.position;

            // Tùy chỉnh hướng dựa trên kiểu Direction được chọn
            switch (_direction)
            {
                case Direction.Y:
                    _dir.y = 0f; // Chỉ giữ hướng XZ
                    break;
                case Direction.XY:
                    _dir.z = 0f; // Chỉ giữ hướng XY
                    break;
                case Direction.YZ:
                    _dir.x = 0f; // Chỉ giữ hướng YZ
                    break;
            }

            // Nếu hướng quá nhỏ, bỏ qua
            if (_dir.sqrMagnitude < 0.0001f)
                return;

            // Quay object về hướng ngược lại với camera (nhìn camera)
            _rotation = Quaternion.LookRotation(-_dir.normalized);
            TransformCached.rotation = _rotation;
        }
    }
}