using Sirenix.OdinInspector;
using UnityEngine;
using Vertx.Debugging;

namespace GOCD.Framework
{
    /// <summary>
    /// Hiển thị vị trí preview các spawn point của config trong Scene View bằng Gizmos.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class GOCDCollectPreview : MonoCached
    {
        [Title("Reference")]
        [SerializeField, AssetsOnly] GOCDCollectConfig _config;
        [SerializeField] Color _colorStart = Color.green;
        [SerializeField] Color _colorEnd = Color.red;

        void OnDrawGizmos()
        {
            if (_config == null || _config.spawnPositions == null || _config.spawnPositions.Count == 0)
                return;

            if (!TryGetComponent<RectTransform>(out var rectTransform))
                return;

            float unitPerPixel = rectTransform.GetUnitPerPixel();

            // Vẽ các điểm spawn preview
            for (int i = 0; i < _config.spawnPositions.Count; i++)
            {
                Vector3 worldPos = transform.TransformPoint(_config.spawnPositions[i]);
                Color c = Color.Lerp(_colorStart, _colorEnd, (float)i / Mathf.Max(1, (_config.spawnPositions.Count - 1)));
                D.raw(new Shape.Circle2D(worldPos, 5.0f * unitPerPixel), c);
            }

            // Vẽ bán kính vùng spawn
            D.raw(new Shape.Circle2D(transform.position, _config.spawnSampleRadius * unitPerPixel), Color.yellow);
        }
    }
}