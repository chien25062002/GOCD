using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GOCD.Framework.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

namespace GOCD.Framework.Internet
{
    public class InternetHelper : MonoCached
    {
        static readonly int RequestTimeOut = 5;
        static readonly float MinCheckInterval = 5f;

        public static View InternetView;
        public static bool IsInternetAvailable { get; set; }

        static InternetHelper _instance;
        static SafeCancellationScope _scope = new();
        static float _lastCheckTime;

        public static void CheckInternetAvailable()
        {
#if INTERNET_CHECK
            GOCDDebug.Log("Check Internet....");
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

        async UniTask HandleCheckInternetConnection(CancellationToken token)
        {
            _lastCheckTime = Time.time;
            var startTime = Time.time;
            const string testUrl = "https://www.google.com";

            IsInternetAvailable = false;

            try
            {
                while (Time.time - startTime < RequestTimeOut)
                {
                    if (token.IsCancellationRequested || this == null) break;

                    using (var request = UnityWebRequest.Get(testUrl))
                    {
                        request.timeout = RequestTimeOut;

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
                            GOCDDebug.LogWarning("Request failed: " + e.Message);
                        }
                    }

                    await UniTask.Delay(500, cancellationToken: token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                GOCDDebug.LogError("Unexpected internet check error: " + e.Message);
            }

            if (this == null || token.IsCancellationRequested) return;

            if (!IsInternetAvailable)
            {
                if (InternetView == null || !InternetView.gameObject.activeInHierarchy)
                {
                    if (!string.IsNullOrEmpty(GOCDFactory.InternetCheck.AssetGUID))
                    {
                        try
                        {
                            var rawView = await ViewHelper.PushAsync(GOCDFactory.InternetCheck)
                                .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
                            InternetView = rawView;
                        }
                        catch (Exception ex)
                        {
                            GOCDDebug.LogWarning("Failed to open InternetView: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    if (InternetView != null && InternetView.gameObject.activeInHierarchy)
                        InternetView.Close();
                }
                catch (Exception ex)
                {
                    GOCDDebug.LogWarning("Failed to close InternetView: " + ex.Message);
                }
            }

            GOCDDebug.Log("InternetStatus: " + IsInternetAvailable);
        }

        public static async UniTask<bool> CheckInternetWithoutView()
        {
#if INTERNET_CHECK
            EnsureInstanceExists();

            const string testUrl = "https://www.google.com";
            IsInternetAvailable = false;

            try
            {
                using var request = UnityWebRequest.Get(testUrl);
                request.timeout = RequestTimeOut;

                await request.SendWebRequest();

                IsInternetAvailable = request.result == UnityWebRequest.Result.Success;
            }
            catch (Exception e)
            {
                GOCDDebug.LogWarning("[CheckInternetWithoutView] Error: " + e.Message);
                IsInternetAvailable = false;
            }

            GOCDDebug.Log("InternetStatus (NoView): " + IsInternetAvailable);
            return IsInternetAvailable;
#else
            return true;
#endif
        }

        public static async UniTask<bool> CheckInternetWithView()
        {
#if INTERNET_CHECK
            EnsureInstanceExists();

            const string testUrl = "https://www.google.com";
            IsInternetAvailable = false;

            try
            {
                using var request = UnityWebRequest.Get(testUrl);
                request.timeout = RequestTimeOut;

                await request.SendWebRequest();

                IsInternetAvailable = request.result == UnityWebRequest.Result.Success;
            }
            catch (Exception e)
            {
                GOCDDebug.LogWarning("[CheckInternetWithView] Error: " + e.Message);
                IsInternetAvailable = false;
            }

            try
            {
                if (!IsInternetAvailable)
                {
                    if (InternetView == null || !InternetView.gameObject.activeInHierarchy)
                    {
                        if (!string.IsNullOrEmpty(CFactory.InternetChecking.AssetGUID))
                        {
                            var rawView = await ViewHelper.PushAsync(CFactory.InternetChecking);
                            InternetView = rawView;
                        }
                    }
                }
                else
                {
                    if (InternetView != null && InternetView.gameObject.activeInHierarchy)
                        InternetView.Close();
                }
            }
            catch (Exception e)
            {
                GOCDDebug.LogWarning("[CheckInternetWithView] View error: " + e.Message);
            }

            GOCDDebug.Log("InternetStatus (WithView): " + IsInternetAvailable);
            return IsInternetAvailable;
#else
            return true;
#endif
        }
    }

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
                    if (!_cts.IsCancellationRequested)
                    {
                        _cts.Cancel();
                    }
    
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
