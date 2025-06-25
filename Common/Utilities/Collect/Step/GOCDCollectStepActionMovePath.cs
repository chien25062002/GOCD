using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Bước hành động di chuyển theo Path (Linear hoặc Bezier).
    /// Hỗ trợ di chuyển khi Spawn hoặc Return về điểm đích.
    /// </summary>
    public class GOCDCollectStepActionMovePath : GOCDCollectStepActionMove
    {
        [SerializeField] PathType _pathType;

        [ValidateInput(nameof(CheckPoints), "Path Type: Cubic Bezier yêu cầu chính xác 2 control points")]
        [SerializeField] Vector3[] _points;

        public override string displayName => "Move Path";

        /// <summary>
        /// Tạo Tween di chuyển theo path tùy chọn giữa 2 điểm, có thể là đường thẳng hoặc cong.
        /// </summary>
        protected override Tween GetTween(GOCDCollectItem item)
        {
            Vector3 posStart = Vector3.zero;
            Vector3 posEnd = Vector3.zero;

            switch (_journey)
            {
                case Journey.Spawn:
                    posEnd = item.TransformCached.localPosition;
                    posStart = _startAtCenter
                        ? Vector3.zero
                        : posEnd + _startOffset * item.rectTransform.GetUnitPerPixel();
                    break;

                case Journey.Return:
                    posEnd = item.TransformCached.position;
                    posStart = item.TransformCached.position;
                    break;
            }

            if (_pathType == PathType.CubicBezier)
            {
                // Chỉ hỗ trợ 2 điểm điều khiển cho Cubic Bezier
                Vector3[] bezierPoints = new Vector3[3];
                bezierPoints[0] = posEnd;
                bezierPoints[1] = posStart + (posEnd - posStart).MultipliedBy(_points[0]);
                bezierPoints[2] = posStart + (posEnd - posStart).MultipliedBy(_points[1]);

                return item.TransformCached.DOPath(bezierPoints, _duration, _pathType, PathMode.Sidescroller2D, 10, Color.red);
            }
            else
            {
                // Linear hoặc CatmullRom path
                Vector3[] pathPoints = new Vector3[_points.Length + 2];
                pathPoints[0] = posStart;
                pathPoints[^1] = posEnd;

                for (int i = 0; i < _points.Length; i++)
                    pathPoints[i + 1] = posStart + (posEnd - posStart).MultipliedBy(_points[i]);

                return item.TransformCached.DOPath(pathPoints, _duration, _pathType, PathMode.Full3D, 10, Color.red);
            }
        }

        /// <summary>
        /// Kiểm tra hợp lệ số lượng điểm đối với Bezier.
        /// </summary>
        bool CheckPoints()
        {
            return _pathType != PathType.CubicBezier || _points.Length == 2;
        }
    }
}
