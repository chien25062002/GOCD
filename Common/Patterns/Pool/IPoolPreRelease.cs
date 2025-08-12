using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework
{
    // Đối tượng hỗ trợ dọn dẹp trước khi trả về pool.
    public interface IPoolPreRelease
    {
        bool IsDirty { get; }          // Cho phép skip cleanup nếu không cần.
        void MarkDirty();              // Gọi khi có thay đổi runtime (vd: Add Rigidbody, đổi layer, v.v.)
        void OnBeforeReleaseToPool();  // Dọn dẹp đồng bộ nhanh.
    }

    // Tuỳ chọn async cleanup nếu cần (IO, chờ 1 frame, tween await, …)
    public interface IAsyncPoolPreRelease
    {
        UniTask OnBeforeReleaseToPoolAsync();
    }
}
