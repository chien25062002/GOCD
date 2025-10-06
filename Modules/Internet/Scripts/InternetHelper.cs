using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GOCD.Framework.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

namespace GOCD.Framework.Internet
{
    /// <summary>
    /// Runtime helper kiểm tra mạng + mở/đóng Internet Popup theo Settings.
    /// - CHỈ dùng Prefab qua PopupManager.Create (không load async Addressables/View).
    /// - Đảm bảo chỉ có 1 popup mở tại 1 thời điểm (registry + atomic guard).
    /// - Internet UI tự đăng ký/huỷ đăng ký để tránh tạo trùng khi người dùng bấm "Try again".
    /// </summary>
    public class InternetHelper : MonoCached
    {
        // ===== Public state =====
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
        // Bất cứ Internet UI nào (Popup) mở sẽ gọi Register; đóng sẽ Unregister.
        static readonly object _uiSync = new();
        static readonly HashSet<int> _uiInstances = new(); // instanceIDs của GameObject chứa UI
        static bool _uiOpenFlag; // nhanh gọn cho IsAnyOpen()

        // ===== Atomic guards: 0 = idle, 1 = busy =====
        static int _openingAny = 0;
        static int _closingAny = 0;

        // ========= Legacy entry (giữ lại cho tương thích) =========
        public static void CheckInternetAvailable()
        {
#if INTERNET_CHECK
            GOCDDebug.Log("[Internet] Check Internet...");
            EnsureInstanceExists();

            if (!_scope.ShouldRun(MinCheckInterval)) return;
            _scope.Run(_instance.HandleCheckInternetConnection);
#endif
        }

        static void EnsureInstanceExists()
        {
            if (_instance == null)
            {
                var go = new GameObject("[InternetHelper]");
                _instance = go.AddComponent<InternetHelper>();
                DontDestroyOnLoad(go);
            }
        }

        // ========= Core check (loop scope cũ) =========
        async UniTask HandleCheckInternetConnection(CancellationToken token)
        {
            var settings = CurrentSettings;
            string testUrl = settings ? settings.testUrl : "https://www.google.com";
            int    timeout = settings ? Mathf.Max(1, settings.requestTimeout) : DefaultRequestTimeOut;

            IsInternetAvailable = false;

#if INTERNET_CHECK
            try
            {
                var startTime = Time.time;
                while (Time.time - startTime < timeout)
                {
                    if (token.IsCancellationRequested || this == null) break;

                    using (var request = UnityWebRequest.Get(testUrl))
                    {
                        request.timeout = timeout;
                        try
                        {
                            await request.SendWebRequest().WithCancellation(token);
                            if (request.result == UnityWebRequest.Result.Success)
                            {
                                IsInternetAvailable = true;
                                break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception e)
                        {
                            GOCDDebug.LogWarning("[Internet] Request failed: " + e.Message);
                        }
                    }

                    await UniTask.Delay(500, cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                GOCDDebug.LogError("[Internet] Unexpected check error: " + e.Message);
            }

            if (this == null || token.IsCancellationRequested) return;

            await HandleOpenOrClosePopup(IsInternetAvailable, settings);
#else
            IsInternetAvailable = true;
#endif
        }

        // ========= Public APIs =========
        public static UniTask<bool> CheckInternetWithoutView() => CheckInternetWithoutPopup(CurrentSettings);
        public static UniTask<bool> CheckInternetWithView()    => CheckInternetWithPopup(CurrentSettings); // giữ tên cũ cho tương thích

        public static async UniTask<bool> CheckInternetWithoutPopup(InternetSettings settings)
        {
#if INTERNET_CHECK
            EnsureInstanceExists();

            string testUrl = settings ? settings.testUrl : "https://www.google.com";
            int    timeout = settings ? Mathf.Max(1, settings.requestTimeout) : DefaultRequestTimeOut;

            try
            {
                using var request = UnityWebRequest.Get(testUrl);
                request.timeout = timeout;
                await request.SendWebRequest();
                IsInternetAvailable = request.result == UnityWebRequest.Result.Success;
            }
            catch (Exception e)
            {
                GOCDDebug.LogWarning("[Internet] CheckInternetWithoutPopup error: " + e.Message);
                IsInternetAvailable = false;
            }

            GOCDDebug.Log("[Internet] Status (NoPopup): " + IsInternetAvailable);
            return IsInternetAvailable;
#else
            IsInternetAvailable = true;
            return true;
#endif
        }

        public static async UniTask<bool> CheckInternetWithPopup(InternetSettings settings)
        {
#if INTERNET_CHECK
            EnsureInstanceExists();

            string testUrl = settings ? settings.testUrl : "https://www.google.com";
            int    timeout = settings ? Mathf.Max(1, settings.requestTimeout) : DefaultRequestTimeOut;

            try
            {
                using var request = UnityWebRequest.Get(testUrl);
                request.timeout = timeout;
                await request.SendWebRequest();
                IsInternetAvailable = request.result == UnityWebRequest.Result.Success;
            }
            catch (Exception e)
            {
                GOCDDebug.LogWarning("[Internet] CheckInternetWithPopup error: " + e.Message);
                IsInternetAvailable = false;
            }

            await _instance.HandleOpenOrClosePopup(IsInternetAvailable, settings);
            GOCDDebug.Log("[Internet] Status (WithPopup): " + IsInternetAvailable);
            return IsInternetAvailable;
#else
            IsInternetAvailable = true;
            return true;
#endif
        }

        // ========= Popup handling (đã khóa single-instance) =========
        async UniTask HandleOpenOrClosePopup(bool hasInternet, InternetSettings settings)
        {
            try
            {
                if (!hasInternet)
                {
                    // Nếu bất kỳ Internet UI nào đang mở -> thôi
                    if (IsAnyOpen()) return;

                    // atomic guard: ngăn 2 luồng cùng vào nhánh mở
                    if (Interlocked.Exchange(ref _openingAny, 1) == 1) return;
                    try
                    {
                        // Double-check sau khi lock
                        if (IsAnyOpen()) return;

                        // Chỉ dùng Prefab -> PopupManager
                        if (settings && settings.internetViewPrefab != null)
                        {
                            ForceClosePopupOnly(); // dọn trường hợp treo ref

                            var popup = PopupManager.Create(settings.internetViewPrefab);
                            InternetPopup = popup;

                            // Đẩy stack (không nhân bản vì Create chỉ 1 lần)
                            PopupManager.PushToStack(popup, isTopHidden: false);

                            // Đăng ký instance (để tránh tạo trùng nếu user spam)
                            TryRegisterFromRoot(popup.gameObject);
                            return;
                        }

                        GOCDDebug.LogWarning("[Internet] Chưa cấu hình Internet Popup Prefab.");
                    }
                    finally
                    {
                        Volatile.Write(ref _openingAny, 0);
                    }
                }
                else
                {
                    CloseAll(); // có mạng -> đóng hết
                }
            }
            catch (Exception ex)
            {
                GOCDDebug.LogWarning("[Internet] Popup handling error: " + ex.Message);
                // Hạ cờ phòng kẹt
                Volatile.Write(ref _openingAny, 0);
            }

            await UniTask.Yield(); // giữ signature async, không làm gì thêm
        }

        // ===== Registry API: gọi từ Internet_View (trên prefab popup) =====
        public static void RegisterUI(Component anyUIRoot)
        {
            if (anyUIRoot == null) return;
            lock (_uiSync)
            {
                int id = anyUIRoot.gameObject.GetInstanceID();
                if (_uiInstances.Add(id))
                {
                    _uiOpenFlag = true;
                }
            }
        }

        public static void UnregisterUI(Component anyUIRoot)
        {
            if (anyUIRoot == null) return;
            lock (_uiSync)
            {
                int id = anyUIRoot.gameObject.GetInstanceID();
                _uiInstances.Remove(id);
                _uiOpenFlag = _uiInstances.Count > 0;
            }
        }

        static void TryRegisterFromRoot(GameObject root)
        {
            if (root == null) return;
            // Nếu prefab có Internet_View, nó sẽ tự đăng ký ở OnEnable.
            // Nhưng để chắc cú (trường hợp inactive child), ta vẫn đánh dấu root hiện hữu:
            RegisterUI(root.transform);
        }

        // ===== Helpers =====
        static bool IsAnyOpen()
        {
            // Ưu tiên cờ nhanh
            if (_uiOpenFlag) return true;

            // Fallback kiểm tra thực thể (trong trường hợp registry chưa kịp cập nhật)
            if (InternetPopup != null && InternetPopup.gameObject && InternetPopup.gameObject.activeInHierarchy)
                return true;

            // Fallback nữa: có Internet_View nào đang active trong scene?
            var any = UnityEngine.Object.FindAnyObjectByType<Internet_View>(FindObjectsInactive.Exclude);
            if (any != null)
            {
                RegisterUI(any.transform);
                return true;
            }
            return false;
        }

        static void ForceClosePopupOnly()
        {
            try
            {
                if (InternetPopup != null && InternetPopup.gameObject)
                {
                    // Tự nó sẽ gọi Unregister ở OnDisable/OnDestroy nếu có Internet_View
                    InternetPopup.Close();
                }
            }
            catch (Exception e)
            {
                GOCDDebug.LogWarning("[Internet] ForceClosePopupOnly error: " + e.Message);
            }
            finally
            {
                InternetPopup = null;
            }
        }

        static void CloseAll()
        {
            if (Interlocked.Exchange(ref _closingAny, 1) == 1) return;
            try
            {
                ForceClosePopupOnly();
                // Dọn registry
                lock (_uiSync)
                {
                    _uiInstances.Clear();
                    _uiOpenFlag = false;
                }
            }
            finally
            {
                Volatile.Write(ref _closingAny, 0);
                Volatile.Write(ref _openingAny, 0);
            }
        }

        // ========= External-control helpers =========
        public static bool IsPopupShowing()
        {
#if INTERNET_CHECK
            EnsureInstanceExists();
            return IsAnyOpen();
#else
    return false;
#endif
        }

        /// <summary>
        /// Đóng popup internet nếu đang mở. Mặc định sẽ dọn registry để lần sau có thể mở lại đúng.
        /// </summary>
        public static void ClosePopupIfShowing(bool clearRegistry = true)
        {
#if INTERNET_CHECK
            EnsureInstanceExists();

            if (Interlocked.Exchange(ref _closingAny, 1) == 1) return;
            try
            {
                ForceClosePopupOnly();

                if (clearRegistry)
                {
                    lock (_uiSync)
                    {
                        _uiInstances.Clear();
                        _uiOpenFlag = false;
                    }
                }
            }
            finally
            {
                Volatile.Write(ref _closingAny, 0);
                Volatile.Write(ref _openingAny, 0);
            }
#endif
        }

        /// <summary>
        /// MỞ popup ngay lập tức KHÔNG kiểm tra mạng (dùng cho Editor/QA).
        /// Tôn trọng prefab trong settings.internetViewPrefab.
        /// </summary>
        public static void ShowPopupDirect(InternetSettings settings = null)
        {
#if INTERNET_CHECK
            EnsureInstanceExists();

            // Nếu đã có popup/Internet UI đang mở thì thôi.
            if (IsAnyOpen()) return;

            // atomic guard: ngăn 2 luồng cùng mở
            if (Interlocked.Exchange(ref _openingAny, 1) == 1) return;
            try
            {
                if (IsAnyOpen()) return;

                // Chỉ dùng Prefab -> PopupManager
                if (settings == null) settings = CurrentSettings;
                if (settings && settings.internetViewPrefab != null)
                {
                    ForceClosePopupOnly(); // dọn ref treo

                    var popup = PopupManager.Create(settings.internetViewPrefab);
                    InternetPopup = popup;

                    PopupManager.PushToStack(popup, isTopHidden: false);

                    TryRegisterFromRoot(popup.gameObject);
                    return;
                }

                GOCDDebug.LogWarning("[Internet] ShowPopupDirect: Chưa cấu hình Internet Popup Prefab.");
            }
            finally
            {
                Volatile.Write(ref _openingAny, 0);
            }
#endif
        }
    }

    // ===== Helper giữ nguyên từ code gốc =====
    public class SafeCancellationScope
    {
        CancellationTokenSource _cts;
        UniTask _runningTask;
        float _lastRunTime;

        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;
        public bool IsRunning => _runningTask.Status == UniTaskStatus.Pending;

        public void CancelAndDispose()
        {
            try
            {
                if (_cts != null)
                {
                    if (!_cts.IsCancellationRequested) _cts.Cancel();
                    _cts.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SafeCancellationScope] Error in CancelAndDispose: {e.Message}");
            }
            finally
            {
                _cts = null;
            }
        }

        public void Run(Func<CancellationToken, UniTask> taskFunc)
        {
            try
            {
                CancelAndDispose();
                _cts = new CancellationTokenSource();
                _lastRunTime = Time.time;
                _runningTask = ExecuteSafe(taskFunc, _cts.Token);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SafeCancellationScope] Run error: {e.Message}");
            }
        }

        public bool ShouldRun(float minInterval)
        {
            return !IsRunning && (Time.time - _lastRunTime >= minInterval);
        }

        async UniTask ExecuteSafe(Func<CancellationToken, UniTask> taskFunc, CancellationToken token)
        {
            try
            {
                await taskFunc(token);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogWarning($"[SafeCancellationScope] ExecuteSafe Exception: {e.Message}");
            }
            finally
            {
                CancelAndDispose();
            }
        }
    }
}
