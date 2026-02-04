using DG.Tweening;
using UnityEngine;

namespace CodeSketch.Core.Extensions
{
    /// <summary>
    /// Extension methods cho SpriteRenderer, hỗ trợ tween và thao tác alpha/size
    /// </summary>
    public static class ExtensionsSpriteRenderer
    {
        /// <summary>
        /// Gán alpha mới cho SpriteRenderer
        /// </summary>
        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        /// <summary>
        /// Tween alpha tới giá trị chỉ định trong khoảng thời gian
        /// </summary>
        public static Tween DOFade(this SpriteRenderer spriteRenderer, float targetAlpha, float duration, Ease ease = Ease.Linear)
        {
            return DOTween.To(() => spriteRenderer.color.a, 
                spriteRenderer.SetAlpha, 
                targetAlpha, 
                duration).SetEase(ease);
        }

        /// <summary>
        /// Tween kích thước (size) của SpriteRenderer tới giá trị đích
        /// </summary>
        public static Tween DOSize(this SpriteRenderer spriteRenderer, Vector2 targetSize, float duration = 0, Ease ease = Ease.Linear)
        {
            return DOVirtual.Vector3(spriteRenderer.size, targetSize, duration, value =>
            {
                spriteRenderer.size = value;
            }).SetEase(ease);
        }

        /// <summary>
        /// Gán trực tiếp kích thước size (không tween)
        /// </summary>
        public static void SetSize(this SpriteRenderer spriteRenderer, Vector2 targetSize)
        {
            spriteRenderer.size = targetSize;
        }
    }
}