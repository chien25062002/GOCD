using System.Collections.Generic;

namespace GOCD.Framework
{
    /// <summary>
    /// Helper để lưu trữ và truy xuất các đích (destination) theo từng config.
    /// Hỗ trợ hệ thống collect có thể tái sử dụng nhiều điểm đích trên scene.
    /// </summary>
    public static class GOCDCollectDestinationHelper
    {
        // Từ khóa config => stack các destination đang active
        static Dictionary<GOCDCollectConfig, Stack<GOCDCollectDestination>> _destinationDict;

        /// <summary>
        /// Ghi nhận một destination mới vào hệ thống
        /// </summary>
        public static void Push(GOCDCollectDestination destination)
        {
            if (_destinationDict == null)
                _destinationDict = new Dictionary<GOCDCollectConfig, Stack<GOCDCollectDestination>>();

            if (!_destinationDict.ContainsKey(destination.config))
                _destinationDict[destination.config] = new Stack<GOCDCollectDestination>();

            _destinationDict[destination.config].Push(destination);
        }

        /// <summary>
        /// Gỡ bỏ một destination khỏi hệ thống
        /// </summary>
        public static void Pop(GOCDCollectDestination destination)
        {
            if (_destinationDict == null)
                return;

            if (_destinationDict.TryGetValue(destination.config, out var stack))
            {
                if (stack != null && stack.Count > 0)
                    stack.Pop();
            }
        }

        /// <summary>
        /// Lấy destination gần nhất theo config (Peek)
        /// </summary>
        public static GOCDCollectDestination Get(GOCDCollectConfig config)
        {
            if (_destinationDict == null)
                return null;

            if (_destinationDict.TryGetValue(config, out var stack))
            {
                if (stack != null && stack.Count > 0)
                    return stack.Peek();
            }

            return null;
        }
    }
}
