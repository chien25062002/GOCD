using UnityEngine;

namespace GOCD.Framework
{
    public static class UtilsHaltonBox
    {
        static int _index = 0;

        /// <summary>
        /// Đặt lại index chuỗi Halton.
        /// </summary>
        public static void Reset()
        {
            _index = 0;
        }

        /// <summary>
        /// Lấy vị trí tiếp theo trong box theo Halton Sequence.
        /// </summary>
        /// <param name="center">Tâm box (gốc phân bố)</param>
        /// <param name="size">Kích thước box (width, height, depth)</param>
        /// <returns>Vector3 vị trí được phân bố đều</returns>
        public static Vector3 NextPosition(Vector3 center, Vector3 size)
        {
            float x = Halton(_index, 2);
            float y = Halton(_index, 3);
            float z = Halton(_index, 5);

            _index++;

            // Đưa về [-0.5, 0.5] để centered quanh tâm
            Vector3 offset = new Vector3(
                (x - 0.5f) * size.x,
                (y - 0.5f) * size.y,
                (z - 0.5f) * size.z
            );

            return center + offset;
        }

        /// <summary>
        /// Hàm sinh số Halton.
        /// </summary>
        static float Halton(int index, int b)
        {
            float result = 0f;
            float f = 1f / b;
            int i = index;
            while (i > 0)
            {
                result += f * (i % b);
                i /= b;
                f /= b;
            }
            return result;
        }
    }
}