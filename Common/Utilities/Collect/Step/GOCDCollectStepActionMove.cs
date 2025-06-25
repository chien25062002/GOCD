using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Step di chuyển object (Spawn từ offset hoặc Return về destination).
    /// </summary>
    public class GOCDCollectStepActionMove : GOCDCollectStepAction
    {
        /// <summary>
        /// Loại hành trình di chuyển: Spawn (xuất hiện từ offset) hoặc Return (trả về đích).
        /// </summary>
        [Serializable]
        public enum Journey
        {
            Spawn = 0,
            Return = 1,
        }

        [SerializeField] protected Journey _journey;

        [Space]

        [ShowIf("@_journey == Journey.Spawn")]
        [SerializeField] protected bool _startAtCenter;

        [ShowIf("@_journey == Journey.Spawn && !_startAtCenter")]
        [SerializeField] protected Vector3 _startOffset;

        /// <summary>
        /// Tạo tween tương ứng với hành trình đã chọn.
        /// </summary>
        protected override Tween GetTween(GOCDCollectItem item)
        {
            switch (_journey)
            {
                case Journey.Spawn:
                    // Vị trí kết thúc là vị trí hiện tại
                    Vector3 endPos = item.TransformCached.localPosition;

                    // Tính vị trí bắt đầu từ offset (theo đơn vị thế giới dựa vào pixel)
                    Vector3 startPos = _startAtCenter
                        ? Vector3.zero
                        : endPos + _startOffset * item.RectTransformCached.GetUnitPerPixel();

                    return item.TransformCached.DOLocalMove(endPos, _duration)
                                               .ChangeStartValue(startPos)
                                               .SetEase(_ease);

                case Journey.Return:
                    return item.TransformCached.DOMove(item.Destination.position, _duration)
                                               .SetEase(_ease);

                default:
                    return null;
            }
        }
    }
}
