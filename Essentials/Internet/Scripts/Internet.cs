#define INTERNET_CHECK
using System;
using System.Threading;
using CodeSketch.Core.Async;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using CodeSketch.Diagnostics;
using CodeSketch.UIPopup;

namespace CodeSketch.Internet
{
    /// <summary>
    /// Internet checker (refactored).
    /// - Check internet continuously
    /// - Spawn / close popup prefab only
    /// - No view script, no registry, no atomic guard
    /// </summary>
    public static class Internet
    {
#if INTERNET_CHECK
        public static bool IsEnabled { get; private set; }

        static Popup InternetPopup;
        static CancelToken CancelToken;

        // ================= INIT =================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            StartCheckLoop();

            PopupManager.EventPopupOpened += Popup_EventOpened;
        }

        static void Popup_EventOpened(Popup popup)
        {
            
        }

        // ================= LOOP =================

        static void StartCheckLoop()
        {
            CancelToken?.Cancel();
            CancelToken = new CancelToken();
            CheckLoop(CancelToken.Token).AttachExternalCancellation(CancelToken.Token);
        }

        static async UniTask CheckLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool hasInternet = await CheckOnce(token);

                if (hasInternet)
                    ClosePopup();
                else
                    OpenPopup();

                await UniTask.Delay(TimeSpan.FromSeconds(InternetSettings.CheckInterval), cancellationToken: token);
            }
        }

        // ================= CHECK =================

        static async UniTask<bool> CheckOnce(CancellationToken token)
        {
            try
            {
                using var request = UnityWebRequest.Get(InternetSettings.Url);
                request.timeout = InternetSettings.RequestTimeout;

                await request.SendWebRequest().WithCancellation(token);

                IsEnabled = request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                IsEnabled = false;
            }

            return IsEnabled;
        }

        // ================= POPUP =================

        static void OpenPopup()
        {
            if (InternetPopup != null)
                return;

            InternetPopup = PopupManager.Create(InternetSettings.Popup);
            CodeSketchDebug.LogRuntime("[Internet] Offline → popup opened", Color.green);
        }

        static void ClosePopup()
        {
            if (InternetPopup == null)
                return;

            InternetPopup.Close();
            InternetPopup = null;
            CodeSketchDebug.LogRuntime("[Internet] Online → popup closed");
        }

        // ================= PUBLIC API =================
        public static bool IsPopupShowing()
        {
            return InternetPopup != null;
        }

        public static void ForceClosePopup()
        {
            ClosePopup();
        }

#else
        public static bool IsEnabled { get; private set; }
#endif
    }
}
