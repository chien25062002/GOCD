using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Halton Sequence và các kỹ thuật tương tự thường được sử dụng để tạo ra các mẫu ngẫu nhiên nhưng phân tán đều.
    ///
    /// Ứng dụng:
    /// 1. Phát tán các đối tượng trong không gian
    /// 2. Tạo điểm xuất phát cho các hệ thống hạt (Particle Systems)
    /// 3. Chiếu sáng và tạo bóng (Light Sampling and Shadowing)
    /// 4. Phát tán điểm spawn cho AI (tránh overlap)
    /// 
    /// Ví dụ sử dụng:
    /// public class AISpawner : MonoBehaviour
    /// {
    ///     public GameObject enemyPrefab;
    ///     public int numberOfEnemies = 10;
    ///     void Start()
    ///     {
    ///         for (int i = 0; i < numberOfEnemies; i++)
    ///         {
    ///             UtilsHaltonSequence.Increment();
    ///             Vector3 spawnPosition = UtilsHaltonSequence.currentPosition * 20.0f;
    ///             Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    ///         }
    ///         UtilsHaltonSequence.Reset();
    ///     }
    /// }
    /// </summary>
    public static class UtilsHaltonSequence
    {
        public static Vector3 currentPosition = Vector3.zero;

        static long _base2 = 0; // Cho trục X
        static long _base3 = 0; // Cho trục Y
        static long _base5 = 0; // Cho trục Z

        /// <summary>
        /// Tăng chỉ số Halton và cập nhật currentPosition.
        /// Mỗi trục có thể bật/tắt bằng tham số.
        /// </summary>
        public static long Increment(bool useX = true, bool useY = true, bool useZ = true)
        {
            float fOneOver3 = 1.0f / 3.0f;
            float fOneOver5 = 1.0f / 5.0f;

            // --- TÍNH CHO TRỤC X (base 2) ---
            long oldBase2 = _base2;
            _base2++;
            long diff = _base2 ^ oldBase2; // XOR để lấy bit khác nhau
            float s = 0.5f;

            if (useX)
            {
                do
                {
                    // Nếu bit cũ là 1 → trừ s, nếu 0 → cộng s
                    currentPosition.x += ((oldBase2 & 1) == 1) ? -s : s;
                    s *= 0.5f;

                    diff >>= 1;
                    oldBase2 >>= 1;
                }
                while (diff > 0);
            }

            // --- TÍNH CHO TRỤC Y (base 3) ---
            long bitmask = 0x3;
            long bitadd = 0x1;
            s = fOneOver3;

            _base3++;

            if (useY)
            {
                while (true)
                {
                    if ((_base3 & bitmask) == bitmask)
                    {
                        _base3 += bitadd;
                        currentPosition.y -= 2 * s;

                        bitmask <<= 2;
                        bitadd <<= 2;
                        s *= fOneOver3;
                    }
                    else
                    {
                        currentPosition.y += s;
                        break;
                    }
                }
            }

            // --- TÍNH CHO TRỤC Z (base 5) ---
            bitmask = 0x7;
            bitadd = 0x3;
            long dmax = 0x5;
            s = fOneOver5;

            _base5++;

            if (useZ)
            {
                while (true)
                {
                    if ((_base5 & bitmask) == dmax)
                    {
                        _base5 += bitadd;
                        currentPosition.z -= 4 * s;

                        bitmask <<= 3;
                        dmax <<= 3;
                        bitadd <<= 3;
                        s *= fOneOver5;
                    }
                    else
                    {
                        currentPosition.z += s;
                        break;
                    }
                }
            }

            return _base2;
        }

        /// <summary>
        /// Đặt lại toàn bộ các chỉ số và vị trí về gốc.
        /// </summary>
        public static void Reset()
        {
            currentPosition = Vector3.zero;
            _base2 = 0;
            _base3 = 0;
            _base5 = 0;
        }
    }
}
