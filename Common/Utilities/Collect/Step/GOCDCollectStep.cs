using System;

namespace GOCD.Framework
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho các bước trong chuỗi hành động Collect.
    /// </summary>
    [Serializable]
    public abstract class GOCDCollectStep
    {
        /// <summary>
        /// Kiểu thêm hành động vào danh sách step.
        /// </summary>
        [Serializable]
        public enum AddType
        {
            Append = 0, // Thêm vào cuối danh sách
            Join = 1,   // Gộp chung với hành động trước đó
            Insert = 2, // Chèn vào vị trí cụ thể
        }

        /// <summary>
        /// Tên hiển thị của step này (sử dụng cho debug hoặc UI).
        /// </summary>
        public abstract string displayName { get; }

        /// <summary>
        /// Áp dụng hành động step này lên một CollectItem cụ thể.
        /// </summary>
        /// <param name="item">CollectItem đang thực thi chuỗi step.</param>
        public abstract void Apply(GOCDCollectItem item);
    }
}