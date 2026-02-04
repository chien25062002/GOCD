using UnityEngine;
using UnityEngine.UI;

namespace CFramework
{
    public static class WorldCanvasManager
    {
        public static Transform Root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (Root != null) return;

            // 💥 Không tìm thấy WorldSpace Canvas → Tạo mới:
            var go = new GameObject("CanvasWorld", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComp = go.GetComponent<Canvas>();
            canvasComp.renderMode = RenderMode.WorldSpace;

            go.transform.localScale = Vector3.one * 0.01f;

            if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
            {
                ((RectTransform)go.transform).sizeDelta = new Vector2(1280, 720);
            }
            else if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            {
                ((RectTransform)go.transform).sizeDelta = new Vector2(720, 1280);
            }

            Object.DontDestroyOnLoad(go); // Đảm bảo CanvasWorld sống qua scene
            Root = go.transform;
        }
    }
}