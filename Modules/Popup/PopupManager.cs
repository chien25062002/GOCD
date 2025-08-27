using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // CanvasScaler, GraphicRaycaster
using GOCD.Framework.Diagnostics;

namespace GOCD.Framework
{
    /// <summary>
    /// Quản lý popup theo stack; tự tìm/cấp root (Canvas) theo ưu tiên:
    /// 0) Root từ các PopupRootSetter đã đăng ký (priority lớn nhất, phải active & enabled)
    /// 1) root được set thủ công (SetRoot) – có thể được set trong eventRootUndefine
    /// 2) Canvas "Overlay" (renderMode = ScreenSpaceOverlay hoặc tên chứa "Overlay") – chỉ lấy canvas enable & activeInHierarchy
    /// 3) Canvas có sortingOrder cao nhất – chỉ lấy canvas enable & activeInHierarchy
    /// 4) Không có thì tự tạo Canvas + EventSystem (CanvasScaler = ScaleWithScreenSize + Expand)
    /// </summary>
    public static class PopupManager
    {
        // ===== Stack quản lý popup =====
        static readonly List<Popup> _mainStack = new List<Popup>();
        static Transform _root;

        public static event Action eventRootUndefine;
        public static event Action<Popup> eventPopupOpened;
        public static event Action<Popup> eventPopupClosed;

        public static int Count => _mainStack.Count;
        public static Popup Top => _mainStack.Count > 0 ? _mainStack[_mainStack.Count - 1] : null;

        /// <summary>Root dùng để chứa popup. Gọi sẽ đảm bảo tồn tại.</summary>
        public static Transform root
        {
            get
            {
                EnsureRoot();
                return _root;
            }
        }

        /// <summary>Đẩy popup lên stack, optional ẩn thằng top cũ.</summary>
        public static void PushToStack(Popup popup, bool isTopHidden = true)
        {
            if (popup == null) return;

            if (_mainStack.Count > 0 && isTopHidden)
                _mainStack[_mainStack.Count - 1].SetEnabled(false);

            _mainStack.Add(popup);
            eventPopupOpened?.Invoke(popup);
        }

        /// <summary>Gỡ popup khỏi stack; nếu là top thì bật lại top mới.</summary>
        public static void PopFromStack(Popup popup)
        {
            if (popup == null) return;

            if (_mainStack.Count == 0)
            {
                GOCDDebug.LogWarning(typeof(PopupManager), "There is no popup in stack");
                return;
            }

            int lastIndex = _mainStack.Count - 1;
            if (!ReferenceEquals(_mainStack[lastIndex], popup))
            {
                GOCDDebug.LogWarning(typeof(PopupManager),
                    $"This popup {popup} is not on top of the stack! Remove it from stack anyway",
                    Color.cyan);
                _mainStack.Remove(popup);
                return;
            }

            // Pop top
            _mainStack.RemoveAt(lastIndex);

            // Enable lại top mới nếu có
            if (_mainStack.Count > 0)
                _mainStack[_mainStack.Count - 1].SetEnabled(true);

            eventPopupClosed?.Invoke(popup);
        }

        /// <summary>Tạo popup dưới root mặc định (tự tìm/cấp nếu thiếu).</summary>
        public static Popup Create(GameObject prefab)
        {
            EnsureRoot(invokeEventIfMissing: true);
            var parent = _root != null ? _root : null;
            var popup = prefab.Create(parent, false).GetComponent<Popup>();
            popup.TransformCached.SetAsLastSibling();
            return popup;
        }

        /// <summary>Tạo popup dưới root chỉ định.</summary>
        public static Popup Create(GameObject prefab, Transform specifiedRoot)
        {
            var popup = prefab.Create(specifiedRoot, false).GetComponent<Popup>();
            popup.TransformCached.SetAsLastSibling();
            return popup;
        }

        /// <summary>Gán root thủ công (ví dụ: Canvas riêng cho UI/Popup).</summary>
        public static void SetRoot(Transform root)
        {
            _root = root;
        }

        /// <summary>Đưa popup thành sibling cuối cùng (lên trên cùng trong layer).</summary>
        public static void BringToFront(Popup popup)
        {
            if (popup != null)
                popup.TransformCached.SetAsLastSibling();
        }

        /// <summary>Xoá sạch stack (không Destroy popup).</summary>
        public static void ClearStack()
        {
            _mainStack.Clear();
        }

        // =========================
        // Root Setter Registry
        // =========================

        static readonly List<PopupRootSetter> _rootCandidates = new List<PopupRootSetter>();

        internal static void RegisterRootCandidate(PopupRootSetter s)
        {
            if (s != null && !_rootCandidates.Contains(s)) _rootCandidates.Add(s);
        }

        internal static void UnregisterRootCandidate(PopupRootSetter s)
        {
            if (s != null) _rootCandidates.Remove(s);
        }

        /// <summary>Chọn Transform của PopupRootSetter có priority cao nhất, chỉ lấy component enable & active.</summary>
        public static bool ResolveRootFromRegisteredCandidates()
        {
            Transform best = null;
            int bestPriority = int.MinValue;

            for (int i = 0; i < _rootCandidates.Count; i++)
            {
                var s = _rootCandidates[i];
                if (s == null) continue;
                if (!s.isActiveAndEnabled) continue;

                if (s.Priority > bestPriority)
                {
                    bestPriority = s.Priority;
                    best = s.transform;
                }
            }

            if (best != null)
            {
                _root = best;
                return true;
            }
            return false;
        }

        // =========================
        // Internal helpers
        // =========================

        static void EnsureRoot(bool invokeEventIfMissing = false)
        {
            if (_root != null)
                return;

            // 0) Thử lấy từ các PopupRootSetter đã đăng ký (ưu tiên priority)
            if (ResolveRootFromRegisteredCandidates())
                return;

            // Cho phép bên ngoài "bơm" root
            if (invokeEventIfMissing) eventRootUndefine?.Invoke();

            if (_root != null) // có thể đã được SetRoot trong event
                return;

            // Nếu event không set -> thử lại theo registry một lần nữa
            if (ResolveRootFromRegisteredCandidates())
                return;

            // 1) Thử tìm Canvas "Overlay" (chỉ lấy canvas enable & active)
            if (TryFindOverlayCanvas(out var overlay))
            {
                _root = overlay.transform;
                return;
            }

            // 2) Thử tìm Canvas có sortingOrder cao nhất (enable & active)
            if (TryFindHighestOrderCanvas(out var highest))
            {
                _root = highest.transform;
                return;
            }

            // 3) Không có -> tạo mới Canvas + EventSystem
            var created = CreateDefaultCanvasAndEventSystem();
            _root = created != null ? created.transform : null;

            if (_root == null && invokeEventIfMissing)
                eventRootUndefine?.Invoke();
        }

        static bool TryFindOverlayCanvas(out Canvas found)
        {
            found = null;
            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(includeInactive: true);
            Canvas candidateByMode = null;
            Canvas candidateByName = null;

            foreach (var c in canvases)
            {
                if (!c.isRootCanvas) continue;
                if (!c.enabled) continue; // bỏ canvas bị disable
                if (!c.gameObject.activeInHierarchy) continue; // bỏ GO bị inactive

                // Ưu tiên RenderMode Overlay
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    if (candidateByMode == null || c.sortingOrder > candidateByMode.sortingOrder)
                        candidateByMode = c;
                }

                // Hoặc tên chứa "Overlay"
                if (c.name.IndexOf("overlay", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (candidateByName == null || c.sortingOrder > candidateByName.sortingOrder)
                        candidateByName = c;
                }
            }

            found = candidateByMode ?? candidateByName;
            return found != null;
        }

        static bool TryFindHighestOrderCanvas(out Canvas found)
        {
            found = null;
            int bestOrder = int.MinValue;

            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(includeInactive: true);
            foreach (var c in canvases)
            {
                if (!c.isRootCanvas) continue;
                if (!c.enabled) continue; // bỏ canvas bị disable
                if (!c.gameObject.activeInHierarchy) continue; // bỏ GO bị inactive

                if (c.sortingOrder >= bestOrder)
                {
                    bestOrder = c.sortingOrder;
                    found = c;
                }
            }

            // Không fallback về Canvas inactive nữa – yêu cầu của bạn là KHÔNG lấy canvas không enable
            return found != null;
        }

        static Canvas CreateDefaultCanvasAndEventSystem()
        {
            // EventSystem (nếu thiếu)
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                UnityEngine.Object.DontDestroyOnLoad(esGO);
            }

            // Canvas
            var go = new GameObject("PopupCanvas_Overlay");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2506;

            // CanvasScaler với mode Expand
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); // tuỳ game bạn chỉnh
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            go.AddComponent<GraphicRaycaster>();
            UnityEngine.Object.DontDestroyOnLoad(go);

            return canvas;
        }
    }
}
