using UnityEngine;
using UnityEngine.UI;

namespace CFramework
{
    public static class WorldCanvasUtility
    {
        public static void Push(Transform transform)
        {
            CheckCanvas();

            if (WorldCanvasManager.Root != null && transform != null)
            {
                transform.SetParent(WorldCanvasManager.Root, false);
                transform.SetAsLastSibling();
            }
        }

        static void CheckCanvas()
        {
            if (WorldCanvasManager.Root == null)
            {
                var go = new GameObject("CanvasWorld", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                WorldCanvasManager.Root = go.transform;
        
                // init canvas
                Canvas canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
        
                go.transform.localScale = Vector3.one * 0.01f;
        
                if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
                {
                    ((RectTransform)go.transform).sizeDelta = new Vector2(1280, 720);
                }
                else if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
                {
                    ((RectTransform)go.transform).sizeDelta = new Vector2(720, 1280);
                }
        
                // 💥 Thêm dòng này để root sống sau scene unload:
                Object.DontDestroyOnLoad(go);
            }
        }
    }
}