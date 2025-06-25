using DG.Tweening;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Bước tween thay đổi Scale đối tượng trong thời gian _duration.
    /// </summary>
    public class GOCDCollectStepActionScale : GOCDCollectStepAction
    {
        [SerializeField] Vector3 _value = Vector3.one;

        /// <summary>
        /// Trả về Tween thay đổi scale đối tượng về giá trị _value.
        /// </summary>
        protected override Tween GetTween(GOCDCollectItem item)
        {
            return item.TransformCached
                .DOScale(_value, _duration)
                .SetEase(_ease);
        }
    }
}