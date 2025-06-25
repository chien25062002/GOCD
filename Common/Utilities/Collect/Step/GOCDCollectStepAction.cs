using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Bước hành động trong chuỗi Collect, cho phép tạo tween với các thuộc tính như ease, duration, loop, update type.
    /// </summary>
    public class GOCDCollectStepAction : GOCDCollectStep
    {
        [SerializeField]
        [HorizontalGroup("AddType")]
        AddType _addType = AddType.Append;

        [SerializeField]
        [HorizontalGroup("AddType"), LabelWidth(75), SuffixLabel("Second(s)", true)]
        [ShowIf("@_addType == GOCDCollectStep.AddType.Insert"), MinValue(0)]
        float _insertTime = 0.0f;

        [SerializeField]
        protected float _duration = 1.0f;

        [SerializeField]
        protected Ease _ease = Ease.Linear;

        [SerializeField]
        [InlineButton("@_isIndependentUpdate = true", Label = "Timescale Based", ShowIf = ("@_isIndependentUpdate == false"))]
        [InlineButton("@_isIndependentUpdate = false", Label = "Independent Update", ShowIf = ("@_isIndependentUpdate == true"))]
        protected UpdateType _updateType = UpdateType.Normal;

        [SerializeField, HideInInspector]
        protected bool _isIndependentUpdate = false;

        [SerializeField, MinValue(0)]
        [HorizontalGroup("Loop")]
        int _loopTime = 0;

        [SerializeField]
        [ShowIf("@_loopTime != 0"), HorizontalGroup("Loop"), LabelWidth(75)]
        LoopType _loopType = LoopType.Restart;

        /// <summary>
        /// Tên hiển thị của step này (thường dùng trong UI hoặc debug).
        /// </summary>
        public override string displayName => ToString();

        /// <summary>
        /// Áp dụng tween vào sequence của item theo cách append, join hoặc insert.
        /// </summary>
        public override void Apply(GOCDCollectItem item)
        {
            Tween tween = GetTween(item);

            tween.SetEase(_ease);
            tween.SetUpdate(_updateType, _isIndependentUpdate);
            tween.SetLoops(_loopTime, _loopType);

            switch (_addType)
            {
                case AddType.Append:
                    item.sequence.Append(tween);
                    break;
                case AddType.Join:
                    item.sequence.Join(tween);
                    break;
                case AddType.Insert:
                    item.sequence.Insert(_insertTime, tween);
                    break;
            }
        }

        /// <summary>
        /// Override hàm này ở subclass để trả về Tween phù hợp cho hành động cụ thể.
        /// </summary>
        protected virtual Tween GetTween(GOCDCollectItem item)
        {
            return null;
        }
    }
}
