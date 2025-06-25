namespace GOCD.Framework.Vibration
{
    /// <summary>
    /// Các kiểu rung khác nhau dùng cho hệ thống haptic feedback.
    /// </summary>
    [System.Serializable]   
    public enum VibrationType 
    {
        Default,           // Rung mặc định đơn giản
        ImpactLight,       // Rung nhẹ
        ImpactMedium,      // Rung trung bình
        ImpactHeavy,       // Rung mạnh
        ImpactSoft,        // Rung nhẹ mượt (thiết bị mới)
        ImpactRigid,       // Rung cứng sắc nét (thiết bị mới)
        Success,           // Rung khi thao tác thành công
        Failure,           // Rung khi thao tác thất bại
        Warning,           // Rung cảnh báo hoặc chú ý
    }
}