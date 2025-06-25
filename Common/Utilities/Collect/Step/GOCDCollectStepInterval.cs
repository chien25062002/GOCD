using DG.Tweening;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Bước chờ (interval) trong sequence - không thực hiện tween, chỉ delay trong thời gian _duration.
    /// </summary>
    public class GOCDCollectStepInterval : GOCDCollectStep
    {
        [SerializeField] float _duration;

        /// <summary>
        /// Hiển thị tên bước trên Editor hoặc trong debug.
        /// </summary>
        public override string displayName => "Interval";

        /// <summary>
        /// Áp dụng bước chờ vào sequence của item.
        /// </summary>
        public override void Apply(GOCDCollectItem item)
        {
            item.sequence.AppendInterval(_duration);
        }
    }
}