using System.Collections.Generic;
using CodeSketch.Mono;
using UnityEngine;

namespace CodeSketch.Utitlities.Billboard
{
    public class CodeSketchBillboardManager : MonoSingleton<CodeSketchBillboardManager>
    {
        protected override bool PersistAcrossScenes => true;
        
        readonly List<CodeSketchBillboard> _billboards = new List<CodeSketchBillboard>();
        static readonly List<int> _invalidIndexes = new(); // Đánh dấu các object đã bị destroy
        
        static Transform _cameraTrans;

        public Transform CameraTrans
        {
            get
            {
                if (_cameraTrans == null)
                {
                    if (Camera.main != null)
                        _cameraTrans = Camera.main.transform;
                    else
                        return null;
                }
                return _cameraTrans;
            }
        }

        void LateUpdate()
        {
            if (CameraTrans == null) return;

            _invalidIndexes.Clear();

            for (int i = 0; i < _billboards.Count; i++)
            {
                var target = _billboards[i];

                // UnityEngine.Object != null vẫn trả true nếu object đã bị Destroy → cần try-catch
                if (target == null)
                {
                    _invalidIndexes.Add(i);
                    continue;
                }

                try
                {
                    target.LookAt(CameraTrans);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[LookAtCameraManager] Exception when LookAt: {ex.Message}");
                    _invalidIndexes.Add(i);
                }
            }

            // Remove các object lỗi (phải từ cuối lên)
            for (int i = _invalidIndexes.Count - 1; i >= 0; i--)
            {
                int index = _invalidIndexes[i];
                if (index >= 0 && index < _billboards.Count)
                {
                    _billboards.RemoveAt(index);
                }
            }
        }

        public static void Register(CodeSketchBillboard billboard)
        {
            if (billboard != null && !SafeInstance._billboards.Contains(billboard))
                SafeInstance._billboards.Add(billboard);
        }

        public static void Unregister(CodeSketchBillboard billboard)
        {
            SafeInstance._billboards.Remove(billboard);
        }
    }
}
