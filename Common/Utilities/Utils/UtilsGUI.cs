using UnityEngine;

namespace GOCD.Framework
{
    public static class UtilsGUI
    {
        // Cache RectTransform của Canvas (ScreenSpaceOverlay)
        static RectTransform Canvas;

        // Cache camera chính
        static Camera Camera;

        /// <summary>
        /// Tự động gọi sau khi scene load để gán Canvas và Camera nếu null.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            if (Canvas == null)
            {
                Canvas[] canvases = Object.FindObjectsOfType<Canvas>();

                if (canvases != null && canvases.Length > 0)
                {
                    // Ưu tiên canvas kiểu Overlay
                    foreach (var canvas in canvases)
                    {
                        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        {
                            Canvas = canvas.transform as RectTransform;
                            break;
                        }
                    }

                    // Nếu không có overlay, lấy canvas đầu tiên
                    if (Canvas == null)
                        Canvas = canvases[0].transform as RectTransform;
                }
            }

            if (Camera == null)
                Camera = Camera.main;

            if (Camera == null)
            {
                var cam = GameObject.FindObjectOfType<Camera>();
                if (cam != null)
                    Camera = cam;
            }
        }

        /// <summary>
        /// Chuyển điểm thế giới sang điểm màn hình (screen point).
        /// </summary>
        public static Vector2 WorldToScreenPoint(Vector3 worldPoint, Camera camera = null)
        {
            return (camera != null ? camera : Camera).WorldToScreenPoint(worldPoint);
        }

        /// <summary>
        /// Chuyển điểm thế giới sang điểm anchored (trong canvas).
        /// </summary>
        public static Vector2 WorldToAnchoredPoint(Vector3 worldPoint, RectTransform canvas, Camera cam = null)
        {
            var screenPoint = (cam != null ? cam : Camera).WorldToScreenPoint(worldPoint);
            return (canvas != null ? canvas : Canvas).InverseTransformPoint(screenPoint);
        }

        /// <summary>
        /// Chuyển RectTransform sang vị trí anchored trong canvas.
        /// </summary>
        public static Vector3 RectTransformToAnchoredPoint(RectTransform rectTransform, RectTransform canvas)
        {
            return (canvas != null ? canvas : Canvas).InverseTransformPoint(rectTransform.ToScreen());
        }

        /// <summary>
        /// Chuyển screen point sang điểm anchored trong canvas.
        /// </summary>
        public static Vector3 ScreenToAnchoredPoint(Vector3 scrPoint, RectTransform canvas)
        {
            return (canvas != null ? canvas : Canvas).InverseTransformPoint(scrPoint);
        }

        /// <summary>
        /// Tạo ray từ RectTransform dựa trên vị trí screen.
        /// </summary>
        public static Ray RectTransformToRay(RectTransform rectTransform, Camera camera = null)
        {
            return (camera != null ? camera : Camera).ScreenPointToRay(rectTransform.ToScreen());
        }
    }
}
