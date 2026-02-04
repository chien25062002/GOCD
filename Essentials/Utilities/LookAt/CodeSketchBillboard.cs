using CodeSketch.Mono;
using UnityEngine;

namespace CodeSketch.Utitlities.Billboard
{
    public class CodeSketchBillboard : MonoBase
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

        protected override void OnEnable()
        {
            base.OnEnable();
            
            CodeSketchBillboardManager.Register(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CodeSketchBillboardManager.Unregister(this);
        }
        void OnDestroy() => CodeSketchBillboardManager.Unregister(this);
    }
}

