using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GOCD.Framework.Utils
{
    /*
     * Button Actions on a World Collider (2D/3D) + Press Visual
     * */
    public class Button_Sprite : MonoBehaviour
    {
        // -------- Original API compatibility --------
        static Func<Camera> _getWorldCamera;

        public static void SetGetWorldCamera(Func<Camera> getWorldCamera)
        {
            _getWorldCamera = getWorldCamera;
        }

        public Action ClickFunc = null;
        public Action MouseRightDownOnceFunc = null;
        public Action MouseRightDownFunc = null;
        public Action MouseRightUpFunc = null;
        public Action MouseDownOnceFunc = null;
        public Action MouseUpOnceFunc = null;
        public Action MouseOverOnceFunc = null;
        public Action MouseOutOnceFunc = null;
        public Action MouseOverOnceTooltipFunc = null;
        public Action MouseOutOnceTooltipFunc = null;

        bool _draggingMouseRight;
        Vector3 _mouseRightDragStart;
        public Action<Vector3, Vector3> MouseRightDragFunc = null;
        public Action<Vector3, Vector3> MouseRightDragUpdateFunc = null;
        public bool triggerMouseRightDragOnEnter = false;

        public enum HoverBehaviour
        {
            Custom,
            Change_Color,
            Change_Image,
            Change_SetActive,
        }

        public HoverBehaviour hoverBehaviourType = HoverBehaviour.Custom;
        Action _hoverBehaviourFuncEnter, _hoverBehaviourFuncExit;

        public Color hoverBehaviour_Color_Enter = new Color(1, 1, 1, 1),
            hoverBehaviour_Color_Exit = new Color(1, 1, 1, 1);

        public SpriteRenderer hoverBehaviour_Image;
        public Sprite hoverBehaviour_Sprite_Exit, hoverBehaviour_Sprite_Enter;
        public bool hoverBehaviour_Move = false;
        public Vector2 hoverBehaviour_Move_Amount = Vector2.zero;
        Vector3 _posExit, _posEnter;
        public bool triggerMouseOutFuncOnClick = false;
        public bool clickThroughUI = false;

        Action _internalOnMouseDownFunc = null, _internalOnMouseEnterFunc = null, _internalOnMouseExitFunc = null;

        // -------- Press visual (click bounce) --------
        public bool pressVisual = true;
        public Transform pressRoot;             // Nếu null sẽ dùng transform của chính object
        [Range(0.5f, 1f)] public float pressScale = 0.95f;
        public float pressOffsetY = -0.02f;
        [Min(0f)] public float pressDuration = 0.06f;
        [Min(0f)] public float releaseDuration = 0.08f;

        Transform _pressT;                      // target transform to animate (pressRoot or self)
        Vector3 _baseScale, _baseLocalPos;      // base state
        Vector3 _fromScale, _toScale;
        Vector3 _fromPos, _toPos;
        float _animT;                           // 0..1
        float _animDuration;
        bool _animPlaying;

        // -------- Optimized UI guard (lazy) --------
        static UIBlockHelper _ui; // only created if used
        static UIBlockHelper UI
        {
            get
            {
                if (_ui == null) _ui = UIBlockHelper.Ensure();
                return _ui;
            }
        }

        #region Monobehaviour

        void Awake()
        {
            if (_getWorldCamera == null)
                SetGetWorldCamera(() => Camera.main); // default World Camera

            _posExit = transform.localPosition;
            _posEnter = transform.localPosition + (Vector3)hoverBehaviour_Move_Amount;
            SetupHoverBehaviour();

            // Press visual init
            _pressT = pressRoot != null ? pressRoot : transform;
            _baseScale = _pressT.localScale;
            _baseLocalPos = _pressT.localPosition;
        }

        void Update()
        {
            UI.TickOncePerFrame();

            // Right-mouse drag update
            if (_draggingMouseRight)
            {
                MouseRightDragUpdateFunc?.Invoke(_mouseRightDragStart, GetWorldPositionFromUI());
            }

            if (Input.GetMouseButtonUp(1))
            {
                if (_draggingMouseRight)
                {
                    _draggingMouseRight = false;
                    MouseRightDragFunc?.Invoke(_mouseRightDragStart, GetWorldPositionFromUI());
                }

                MouseRightUpFunc?.Invoke();
            }

            // Press visual animation tick (no GC)
            if (_animPlaying)
            {
                if (_animDuration <= 0f)
                {
                    // snap
                    _pressT.localScale = _toScale;
                    _pressT.localPosition = _toPos;
                    _animPlaying = false;
                }
                else
                {
                    _animT += Time.unscaledDeltaTime / _animDuration; // unscaled để không phụ thuộc timeScale
                    if (_animT >= 1f) _animT = 1f;

                    // ease-out-quadratic nhẹ
                    float t = 1f - (1f - _animT) * (1f - _animT);

                    _pressT.localScale = Vector3.LerpUnclamped(_fromScale, _toScale, t);
                    _pressT.localPosition = Vector3.LerpUnclamped(_fromPos, _toPos, t);

                    if (_animT >= 1f) _animPlaying = false;
                }
            }
        }

        void LateUpdate()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
            TouchRouter.Ensure(_getWorldCamera).Tick();
#endif
        }

        void OnDisable()
        {
            // đảm bảo trả về trạng thái gốc khi disable
            if (_pressT != null)
            {
                _pressT.localScale = _baseScale;
                _pressT.localPosition = _baseLocalPos;
                _animPlaying = false;
            }
        }

        #endregion

        // -------- Hover behavior helpers --------
        public void SetHoverBehaviourChangeColor(Color colorOver, Color colorOut)
        {
            hoverBehaviourType = HoverBehaviour.Change_Color;
            hoverBehaviour_Color_Enter = colorOver;
            hoverBehaviour_Color_Exit = colorOut;
            if (hoverBehaviour_Image == null) hoverBehaviour_Image = transform.GetComponent<SpriteRenderer>();
            hoverBehaviour_Image.color = hoverBehaviour_Color_Exit;
            SetupHoverBehaviour();
        }

        #region Pointer Events

        void OnMouseDown()
        {
            if (!clickThroughUI && UI.IsBlockedMouse()) return;

            _internalOnMouseDownFunc?.Invoke();
            ClickFunc?.Invoke();
            MouseDownOnceFunc?.Invoke();

            _PressVisualDown();

            if (triggerMouseOutFuncOnClick) OnMouseExit();
        }

        public void Manual_OnMouseExit() => OnMouseExit();

        void OnMouseUp()
        {
            if (!clickThroughUI && UI.IsBlockedMouse())
            {
                UI.ReleaseMouseIfUp();
                _PressVisualUp(); // đảm bảo nhả visual
                return;
            }

            MouseUpOnceFunc?.Invoke();
            UI.ReleaseMouseIfUp();
            _PressVisualUp();
        }

        void OnMouseEnter()
        {
            if (!clickThroughUI && UI.IsPointerOverUI_MouseThisFrame) return;

            _internalOnMouseEnterFunc?.Invoke();
            if (hoverBehaviour_Move) transform.localPosition = _posEnter;
            _hoverBehaviourFuncEnter?.Invoke();
            MouseOverOnceFunc?.Invoke();
            MouseOverOnceTooltipFunc?.Invoke();
        }

        void OnMouseExit()
        {
            _internalOnMouseExitFunc?.Invoke();
            if (hoverBehaviour_Move) transform.localPosition = _posExit;
            _hoverBehaviourFuncExit?.Invoke();
            MouseOutOnceFunc?.Invoke();
            MouseOutOnceTooltipFunc?.Invoke();
        }

        void OnMouseOver()
        {
            if (!clickThroughUI && UI.IsBlockedMouse()) return;

            if (Input.GetMouseButton(1))
            {
                MouseRightDownFunc?.Invoke();
                if (!_draggingMouseRight && triggerMouseRightDragOnEnter)
                {
                    _draggingMouseRight = true;
                    _mouseRightDragStart = GetWorldPositionFromUI();
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                _draggingMouseRight = true;
                _mouseRightDragStart = GetWorldPositionFromUI();
                MouseRightDownOnceFunc?.Invoke();
            }
        }

        #endregion

        void SetupHoverBehaviour()
        {
            switch (hoverBehaviourType)
            {
                case HoverBehaviour.Change_Color:
                    _hoverBehaviourFuncEnter = OnHoverEnter_Color;
                    _hoverBehaviourFuncExit = OnHoverExit_Color;
                    break;
                case HoverBehaviour.Change_Image:
                    _hoverBehaviourFuncEnter = OnHoverEnter_Image;
                    _hoverBehaviourFuncExit = OnHoverExit_Image;
                    break;
                case HoverBehaviour.Change_SetActive:
                    _hoverBehaviourFuncEnter = OnHoverEnter_SetActive;
                    _hoverBehaviourFuncExit = OnHoverExit_SetActive;
                    break;
                case HoverBehaviour.Custom:
                default:
                    _hoverBehaviourFuncEnter = null;
                    _hoverBehaviourFuncExit = null;
                    break;
            }
        }

        void OnHoverEnter_Color() { if (hoverBehaviour_Image != null) hoverBehaviour_Image.color = hoverBehaviour_Color_Enter; }
        void OnHoverExit_Color() { if (hoverBehaviour_Image != null) hoverBehaviour_Image.color = hoverBehaviour_Color_Exit; }
        void OnHoverEnter_Image() { if (hoverBehaviour_Image != null) hoverBehaviour_Image.sprite = hoverBehaviour_Sprite_Enter; }
        void OnHoverExit_Image() { if (hoverBehaviour_Image != null) hoverBehaviour_Image.sprite = hoverBehaviour_Sprite_Exit; }
        void OnHoverEnter_SetActive() { if (hoverBehaviour_Image != null) hoverBehaviour_Image.gameObject.SetActive(true); }
        void OnHoverExit_SetActive() { if (hoverBehaviour_Image != null) hoverBehaviour_Image.gameObject.SetActive(false); }

        static Vector3 GetWorldPositionFromUI()
        {
            Vector3 worldPosition = _getWorldCamera().ScreenToWorldPoint(Input.mousePosition);
            return worldPosition;
        }

        // -------- Press visual helpers --------
        void _PressVisualDown()
        {
            if (!pressVisual) return;
            if (_pressT == null) { _pressT = pressRoot != null ? pressRoot : transform; }

            // target
            Vector3 targetScale = _baseScale * pressScale;
            Vector3 targetPos = _baseLocalPos + new Vector3(0f, pressOffsetY, 0f);

            // animate from current to target
            _fromScale = _pressT.localScale;
            _toScale = targetScale;
            _fromPos = _pressT.localPosition;
            _toPos = targetPos;
            _animT = 0f;
            _animDuration = pressDuration;
            _animPlaying = true;
        }

        void _PressVisualUp()
        {
            if (!pressVisual) return;
            if (_pressT == null) { _pressT = pressRoot != null ? pressRoot : transform; }

            // animate back to base
            _fromScale = _pressT.localScale;
            _toScale = _baseScale;
            _fromPos = _pressT.localPosition;
            _toPos = _baseLocalPos;
            _animT = 0f;
            _animDuration = releaseDuration;
            _animPlaying = true;
        }

        // ===================== Internal Helpers =====================
        sealed class UIBlockHelper : MonoBehaviour
        {
            static UIBlockHelper _instance;
            static PointerEventData _peCached;
            static readonly List<RaycastResult> _hits = new List<RaycastResult>(8);

            int _lastFrame = -1;
            bool _mouseOverUIThisFrame;
            bool _mouseCapturedByUI;

            public bool IsPointerOverUI_MouseThisFrame => _mouseOverUIThisFrame;

            public static UIBlockHelper Ensure()
            {
                if (_instance != null) return _instance;
                var go = new GameObject("[ButtonSprite.UIBlockHelper]");
                go.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<UIBlockHelper>();
                return _instance;
            }

            public void TickOncePerFrame()
            {
                if (Time.frameCount == _lastFrame) return;
                _lastFrame = Time.frameCount;

                _mouseOverUIThisFrame = RaycastUIAt(Input.mousePosition); // NOTE: Unity uses Input.mousePosition (case)
            }

            bool RaycastUIAt(Vector2 screenPos)
            {
                var es = EventSystem.current;
                if (es == null) return false;

                if (_peCached == null) _peCached = new PointerEventData(es);
                _peCached.Reset();
                _peCached.position = screenPos;

                _hits.Clear();
                es.RaycastAll(_peCached, _hits);

                // capture on mouse down if started over UI
                if (Input.GetMouseButtonDown(0) && _hits.Count > 0) _mouseCapturedByUI = true;
                if (Input.GetMouseButtonUp(0)) _mouseCapturedByUI = false;

                return _hits.Count > 0;
            }

            public bool IsBlockedMouse() => _mouseCapturedByUI || _mouseOverUIThisFrame;

            public void ReleaseMouseIfUp()
            {
                if (Input.GetMouseButtonUp(0)) _mouseCapturedByUI = false;
            }
        }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        sealed class TouchRouter
        {
            static TouchRouter _inst;
            readonly Dictionary<int, Button_Sprite> _downMap = new Dictionary<int, Button_Sprite>(8);
            Func<Camera> _getCam;

            Camera Cam => _getCam != null ? _getCam() : Camera.main;

            public static TouchRouter Ensure(Func<Camera> getCam)
            {
                if (_inst == null) _inst = new TouchRouter { _getCam = getCam };
                return _inst;
            }

            public void Tick()
            {
                int tc = Input.touchCount;
                if (tc == 0) return;

                var es = EventSystem.current;
                for (int i = 0; i < tc; i++)
                {
                    var t = Input.GetTouch(i);
                    int id = t.fingerId;

                    switch (t.phase)
                    {
                        case TouchPhase.Began:
                            if (es != null && es.IsPointerOverGameObject(id))
                            {
                                _downMap.Remove(id);
                                continue;
                            }

                            var btnDown = RaycastButton(t.position);
                            if (btnDown != null)
                            {
                                _downMap[id] = btnDown;

                                // visual + callback
                                btnDown._PressVisualDown();
                                btnDown.MouseDownOnceFunc?.Invoke();
                            }
                            break;

                        case TouchPhase.Ended:
                        case TouchPhase.Canceled:
                            Button_Sprite pressed = null;
                            _downMap.TryGetValue(id, out pressed);

                            if (pressed != null)
                            {
                                // visual up first
                                pressed._PressVisualUp();
                                pressed.MouseUpOnceFunc?.Invoke();
                            }

                            var btnUp = RaycastButton(t.position);
                            if (pressed != null && btnUp == pressed)
                            {
                                pressed.ClickFunc?.Invoke();
                            }

                            _downMap.Remove(id);
                            break;
                    }
                }
            }

            Button_Sprite RaycastButton(Vector2 screenPos)
            {
                var cam = Cam;
                if (cam == null) return null;
                var ray = cam.ScreenPointToRay(screenPos);

                var hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
                if (hit2D.collider != null)
                {
                    var bs = hit2D.collider.GetComponentInParent<Button_Sprite>();
                    if (bs != null && bs.enabled && bs.gameObject.activeInHierarchy) return bs;
                }

                if (Physics.Raycast(ray, out var hit3D, Mathf.Infinity))
                {
                    var bs = hit3D.collider.GetComponentInParent<Button_Sprite>();
                    if (bs != null && bs.enabled && bs.gameObject.activeInHierarchy) return bs;
                }

                return null;
            }
        }
#endif
    }
}
