using UnityEngine;

namespace GOCD.Framework.ChainOfResponsibility
{
    public interface IHandler<T>
    {
        void Handle(T request); // Xử lý yêu cầu nếu hợp lệ
        void PostProcess(T request); // // Thực hiện các hành động bổ sung sau khi xử lý xong request (Ví dụ trả nhiệm vụ nhận thưởng)
        void SetNext(IHandler<T> nextHandler); // Đặt handler kế tiếp
        bool CanHandle(T request); // Kiểm tra xem handler này có xử lý được request hay không
    }
}
