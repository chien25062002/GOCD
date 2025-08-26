using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GOCD.Framework.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

namespace GOCD.Framework.Internet {
    /// <summary>
    /// Runtime helper kiểm tra mạng + mở/đóng InternetView theo Settings.
    /// - Ưu tiên Prefab (PopupManager.Create)
    /// - Nếu không có Prefab thì Addressables (ViewHelper.PushAsync)
    /// - Đảm bảo chỉ có 1 popup HOẶC 1 view mở tại 1 thời điểm
    /// - Có registry để Internet UI tự đăng ký, tránh tạo trùng khi người dùng bấm "Try again"
    /// </summary>
    public class InternetHelper : MonoCached {
        // ===== Public state =====
        /// <summary>View mở qua Addressables</summary>
        public static View  InternetView;
        /// <summary>Popup mở qua Prefab</summary>
        public static Popup InternetPopup;
        public static bool  IsInternetAvailable { get; private set; }

        /// <summary>Settings hiện hành cho API paramless</summary>
        public static InternetSettings CurrentSettings { get; set; }

        // ===== Internal fields =====
        static readonly int   DefaultRequestTimeOut = 5;
        static readonly float MinCheckInterval      = 5f;

        static InternetHelper        _instance;
        static SafeCancellationScope _scope = new();

        // ===== UI Single-instance Registry =====
        // Bất cứ Internet UI nào (Popup/View) mở sẽ gọi Register; đóng sẽ Unregister.
        static readonly object _uiSync = new();
        static readonly HashSet<int> _uiInstances = new(); // instanceIDs của GameObject chứa UI
        static bool _uiOpenFlag; // nhanh gọn cho IsAnyOpen()

        // ===== Atomic guards: 0 = idle, 1 = busy =====
        static int _openingAny = 0;
        static int _closingAny = 0;

        // ========= Legacy entry (giữ lại cho tương thích) =========
        public static void CheckInternetAvailable() {
#if INTERNET_CHECK
            GOCDDebug.Log("[Internet] Check Internet...");
            EnsureInstanceExists();

            if (!_scope.ShouldRun(MinCheckInterval)) return;
            _scope.Run(_instance.HandleCheckInternetConnection);
#endif
        }

        static void EnsureInstanceExists()
        {
            if (_instance == null) {
                var go = new GameObject("[InternetHelper]");
                _instance = go.AddComponent<InternetHelper>();
                DontDestroyOnLoad(go);
            }
        }

        // ========= Core check (loop scope cũ) =========
        async UniTask HandleCheckInternetConnection(CancellationToken token) {
            var settings = CurrentSettings;
            string testUrl = settings ? settings.testUrl : "https://www.google.com";
            int    timeout = settings ? Mathf.Max(1, settings.requestTimeout) : DefaultRequestTimeOut;

            IsInternetAvailable = false;

#if INTERNET_CHECK
            try {
                var startTime = Time.time;
                while (Time.time - startTime < timeout) {
                    if (token.IsCancellationRequested || this == null) break;

                    using (var request = UnityWebRequest.Get(testUrl)) {
                        request.timeout = timeout;
                        try {
                            await request.SendWebRequest().WithCancellation(token);
                            if (request.result == UnityWebRequest.Result.Success) {
                                IsInternetAvailable = true;
                                break;
                            }
                        } catch (OperationCanceledException) {
                            break;
                        } catch (Exception e) {
                            GOCDDebug.LogWarning("[Internet] Request failed: " + e.Message);
                        }
                    }

                    await UniTask.Delay(500, cancellationToken: token);
                }
            } catch (OperationCanceledException) {
            } catch (Exception e) {
                GOCDDebug.LogError("[Internet] Unexpected check error: " + e.Message);
            }

            if (this == null || token.IsCancellationRequested) return;

            await HandleOpenOrCloseView(IsInternetAvailable, settings);
#else
            IsInternetAvailable = true;
#endif
        }

        // ========= New public APIs (khuyên dùng) =========
        public static UniTask<bool> CheckInternetWithoutView() => CheckInternetWithoutView(CurrentSettings);
        public static UniTask<bool> CheckInternetWithView()    => CheckInternetWithView(CurrentSettings);

        public static async UniTask<bool> CheckInternetWithoutView(InternetSettings settings) {
#if INTERNET_CHECK
            EnsureInstanceExists();

            string testUrl = settings ? settings.testUrl : "https://www.google.com";
            int    timeout = settings ? Mathf.Max(1, settings.requestTimeout) : DefaultRequestTimeOut;

            try {
                using var request = UnityWebRequest.Get(testUrl);
                request.timeout = timeout;
                await request.SendWebRequest();
                IsInternetAvailable = request.result == UnityWebRequest.Result.Success;
            } catch (Exception e) {
                GOCDDebug.LogWarning("[Internet] CheckInternetWithoutView error: " + e.Message);
                IsInternetAvailable = false;
            }

            GOCDDebug.Log("[Internet] Status (NoView): " + IsInternetAvailable);
            return IsInternetAvailable;
#else
            IsInternetAvailable = true;
            return true;
#endif
        }

        public static async UniTask<bool> CheckInternetWithView(InternetSettings settings) {
#if INTERNET_CHECK
            EnsureInstanceExists();

            string testUrl = settings ? settings.testUrl : "https://www.google.com";
            int    timeout = settings ? Mathf.Max(1, settings.requestTimeout) : DefaultRequestTimeOut;

            try {
                using var request = UnityWebRequest.Get(testUrl);
                request.timeout = timeout;
                await request.SendWebRequest();
                IsInternetAvailable = request.result == UnityWebRequest.Result.Success;
            } catch (Exception e) {
                GOCDDebug.LogWarning("[Internet] CheckInternetWithView error: " + e.Message);
                IsInternetAvailable = false;
            }

            await _instance.HandleOpenOrCloseView(IsInternetAvailable, settings);
            GOCDDebug.Log("[Internet] Status (WithView): " + IsInternetAvailable);
            return IsInternetAvailable;
#else
            IsInternetAvailable = true;
            return true;
#endif
        }

        // ========= View handling (đã khóa single-instance) =========
        async UniTask HandleOpenOrCloseView(bool hasInternet, InternetSettings settings) {
            try {
                if (!hasInternet) {
                    // Nếu bất kỳ Internet UI nào đang đăng ký là mở -> thôi
                    if (IsAnyOpen()) return;

                    // atomic guard: ngăn 2 luồng cùng vào nhánh mở
                    if (Interlocked.Exchange(ref _openingAny, 1) == 1) return;
                    try {
                        // Double-check sau khi lock
                        if (IsAnyOpen()) return;

                        // 1) Ưu tiên Prefab -> PopupManager
                        if (settings && settings.internetViewPrefab != null) {
                            // Đóng loại kia nếu lỡ còn treo ref
                            ForceCloseViewOnly();

                            var popup = PopupManager.Create(settings.internetViewPrefab);
                            InternetPopup = popup;

                            // Đẩy stack (không nhân bản vì Create chỉ 1 lần)
                            PopupManager.PushToStack(popup, isTopHidden: false);

                            // Đăng ký instance nếu prefab có Internet_View
                            TryRegisterFromRoot(popup.gameObject);
                            return;
                        }

                        // 2) Nếu không có Prefab thì dùng Addressables -> ViewHelper
#if GOCD_VIEW_HELPER_GUID
                        bool hasGuid = settings && !string.IsNullOrEmpty(settings.factoryGuid);
#else
                        bool hasGuid = false;
#endif
                        if (settings && settings.addressableView != null && settings.addressableView.RuntimeKeyIsValid()) {
                            ForceClosePopupOnly();
                            var rawView = await ViewHelper.PushAsync(settings.addressableView)
                                .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
                            InternetView = rawView;

                            TryRegisterFromRoot(rawView.gameObject);
                            return;
                        }

#if GOCD_VIEW_HELPER_GUID
                        if (hasGuid) {
                            ForceClosePopupOnly();
                            var rawView = await ViewHelper.PushAsync(settings.factoryGuid)
                                .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
                            InternetView = rawView;

                            TryRegisterFromRoot(rawView.gameObject);
                            return;
                        }
#endif

                        GOCDDebug.LogWarning("[Internet] Chưa cấu hình InternetView (Prefab/Addressable/Factory GUID).");
                    } finally {
                        Volatile.Write(ref _openingAny, 0);
                    }
                } else {
                    CloseAll(); // có mạng -> đóng hết
                }
            } catch (Exception ex) {
                GOCDDebug.LogWarning("[Internet] View handling error: " + ex.Message);
                // Hạ cờ phòng kẹt
                Volatile.Write(ref _openingAny, 0);
            }
        }

        // ===== Registry API: gọi từ Internet_View =====
        public static void RegisterUI(Component anyUIRoot) {
            if (anyUIRoot == null) return;
            lock (_uiSync) {
                int id = anyUIRoot.gameObject.GetInstanceID();
                if (_uiInstances.Add(id)) {
                    _uiOpenFlag = true;
                }
            }
        }

        public static void UnregisterUI(Component anyUIRoot) {
            if (anyUIRoot == null) return;
            lock (_uiSync) {
                int id = anyUIRoot.gameObject.GetInstanceID();
                _uiInstances.Remove(id);
                _uiOpenFlag = _uiInstances.Count > 0;
            }
        }

        static void TryRegisterFromRoot(GameObject root) {
            if (root == null) return;
            // Nếu prefab/view có Internet_View, nó sẽ tự đăng ký ở OnEnable.
            // Nhưng để chắc cú (trường hợp inactive child), ta vẫn đánh dấu root hiện hữu:
            RegisterUI(root.transform);
        }

        // ===== Helpers =====
        static bool IsAnyOpen() {
            // Ưu tiên cờ nhanh
            if (_uiOpenFlag) return true;

            // Fallback kiểm tra thực thể (trong trường hợp registry chưa kịp cập nhật)
            if (InternetPopup != null && InternetPopup.gameObject && InternetPopup.gameObject.activeInHierarchy)
                return true;
            if (InternetView  != null && InternetView.gameObject  && InternetView.gameObject.activeInHierarchy)
                return true;

            // Fallback nữa: có Internet_View nào đang active trong scene?
            var any = UnityEngine.Object.FindAnyObjectByType<Internet_View>(FindObjectsInactive.Exclude);
            if (any != null) {
                RegisterUI(any.transform);
                return true;
            }
            return false;
        }

        static void ForceClosePopupOnly() {
            try {
                if (InternetPopup != null && InternetPopup.gameObject) {
                    // Tự nó sẽ gọi Unregister ở OnDisable/OnDestroy nếu có Internet_View
                    InternetPopup.Close();
                }
            } catch (Exception e) {
                GOCDDebug.LogWarning("[Internet] ForceClosePopupOnly error: " + e.Message);
            } finally {
                InternetPopup = null;
            }
        }

        static void ForceCloseViewOnly() {
            try {
                if (InternetView != null && InternetView.gameObject) {
                    // Tự nó sẽ gọi Unregister ở OnDisable/OnDestroy nếu có Internet_View
                    InternetView.Close();
                }
            } catch (Exception e) {
                GOCDDebug.LogWarning("[Internet] ForceCloseViewOnly error: " + e.Message);
            } finally {
                InternetView = null;
            }
        }

        static void CloseAll() {
            if (Interlocked.Exchange(ref _closingAny, 1) == 1) return;
            try {
                ForceClosePopupOnly();
                ForceCloseViewOnly();
                // Dọn registry
                lock (_uiSync) {
                    _uiInstances.Clear();
                    _uiOpenFlag = false;
                }
            } finally {
                Volatile.Write(ref _closingAny, 0);
                Volatile.Write(ref _openingAny, 0);
            }
        }
    }

    // ===== Helper giữ nguyên từ code gốc =====
    public class SafeCancellationScope {
        CancellationTokenSource _cts;
        UniTask _runningTask;
        float _lastRunTime;

        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;
        public bool IsRunning => _runningTask.Status == UniTaskStatus.Pending;

        public void CancelAndDispose() {
            try {
                if (_cts != null) {
                    if (!_cts.IsCancellationRequested) _cts.Cancel();
                    _cts.Dispose();
                }
            } catch (Exception e) {
                Debug.LogWarning($"[SafeCancellationScope] Error in CancelAndDispose: {e.Message}");
            } finally {
                _cts = null;
            }
        }

        public void Run(Func<CancellationToken, UniTask> taskFunc) {
            try {
                CancelAndDispose();
                _cts = new CancellationTokenSource();
                _lastRunTime = Time.time;
                _runningTask = ExecuteSafe(taskFunc, _cts.Token);
            } catch (Exception e) {
                Debug.LogWarning($"[SafeCancellationScope] Run error: {e.Message}");
            }
        }

        public bool ShouldRun(float minInterval) {
            return !IsRunning && (Time.time - _lastRunTime >= minInterval);
        }

        async UniTask ExecuteSafe(Func<CancellationToken, UniTask> taskFunc, CancellationToken token) {
            try {
                await taskFunc(token);
            } catch (OperationCanceledException) {
            } catch (Exception e) {
                Debug.LogWarning($"[SafeCancellationScope] ExecuteSafe Exception: {e.Message}");
            } finally {
                CancelAndDispose();
            }
        }
    }
}
