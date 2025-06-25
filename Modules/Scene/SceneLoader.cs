using System;
using DG.Tweening;
using GOCD.Framework.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace GOCD.Framework
{
    /// <summary>
    /// Quản lý việc chuyển cảnh với hiệu ứng fade-in/fade-out.
    /// </summary>
    public class SceneLoader : MonoCached
    {
        static readonly string s_fadeIn = "FadeIn";
        static readonly string s_fadeOut = "FadeOut";

        enum State
        {
            Idle,
            FadeIn,
            Loading,
            FadeOut
        }

        float _fadeInAnimSpeed;
        float _fadeOutAnimSpeed;
        int _sceneIndex;
        internal LoadSceneMode _mode = LoadSceneMode.Single;

        AsyncOperation _sceneAsync;
        Animator _animator;
        StateMachine<State> _stateMachine;
        Tween _tween;

        public event Action OnComplete;

        #region MonoBehaviour

        void Start()
        {
            GameObjectCached.SetActive(false);
        }

        void OnDestroy()
        {
            _tween?.Kill();
        }

        void Update()
        {
            _stateMachine?.Update();
        }

        #endregion

        #region States

        void State_OnFadeInStart()
        {
            _sceneAsync = SceneManager.LoadSceneAsync(_sceneIndex, _mode);
            _sceneAsync.allowSceneActivation = false;

            _animator.Play(s_fadeIn);
            _animator.speed = _fadeInAnimSpeed;

            _tween?.Kill();
            _tween = DOVirtual.DelayedCall(
                GOCDFactory.sceneTransitionFadeInDuration + GOCDFactory.sceneTransitionLoadDuration,
                OnFadeInComplete,
                ignoreTimeScale: true);
        }

        void OnFadeInComplete()
        {
            _stateMachine.CurrentState = State.Loading;
            _sceneAsync.allowSceneActivation = true;
        }

        void State_OnLoadingUpdate()
        {
            if (_sceneAsync?.isDone == true)
            {
                _stateMachine.CurrentState = State.FadeOut;
                OnComplete?.Invoke();
                OnComplete = null;
            }
        }

        void State_OnFadeOutStart()
        {
            _animator.Play(s_fadeOut);
            _animator.speed = _fadeOutAnimSpeed;

            _tween?.Kill();
            _tween = DOVirtual.DelayedCall(
                GOCDFactory.sceneTransitionFadeOutDuration,
                OnFadeOutComplete);
        }

        void OnFadeOutComplete()
        {
            _stateMachine.CurrentState = State.Idle;
            GameObjectCached.SetActive(false);
        }

        #endregion

        #region Public API

        public void Load(int sceneIndex, LoadSceneMode mode = LoadSceneMode.Single, Action onComplete = null)
        {
            if (_stateMachine.CurrentState != State.Idle)
            {
                GOCDDebug.Log<SceneLoader>("A scene is loading, can't execute load scene command!", Color.cyan);
                return;
            }

            OnComplete = onComplete;
            GameObjectCached.SetActive(true);

            _sceneIndex = sceneIndex;
            _mode = mode;
            _stateMachine.CurrentState = State.FadeIn;
        }

        public void Reload()
        {
            Load(SceneManager.GetActiveScene().buildIndex);
        }

        public void Construct()
        {
            _animator = GetComponentInChildren<Animator>();
            CalculateAnimSpeed();

            _stateMachine = new StateMachine<State>();
            _stateMachine.AddState(State.FadeIn, State_OnFadeInStart);
            _stateMachine.AddState(State.Loading, State_OnLoadingUpdate);
            _stateMachine.AddState(State.FadeOut, State_OnFadeOutStart);
            _stateMachine.AddState(State.Idle);

            _stateMachine.CurrentState = State.Idle;
        }

        #endregion

        /// <summary>
        /// Tính toán tốc độ animation để khớp thời gian theo config.
        /// </summary>
        void CalculateAnimSpeed()
        {
            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == s_fadeIn)
                    _fadeInAnimSpeed = clip.length / GOCDFactory.sceneTransitionFadeInDuration;
                else if (clip.name == s_fadeOut)
                    _fadeOutAnimSpeed = clip.length / GOCDFactory.sceneTransitionFadeOutDuration;
            }
        }
    }

    /// <summary>
    /// Helper cho việc load cảnh, đơn giản hóa API và hỗ trợ async/fade.
    /// </summary>
    public static class SceneLoaderHelper
    {
        static SceneLoader _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void LazyInit()
        {
            if (_instance != null) return;

            if (GOCDFactory.sceneTransitionPrefab == null)
            {
                GOCDDebug.LogWarning<SceneLoader>("Unassigned prefab!", Color.cyan);
                return;
            }

            _instance = GOCDFactory.sceneTransitionPrefab.Create().GetComponent<SceneLoader>();
            _instance.Construct();
            Object.DontDestroyOnLoad(_instance.GameObjectCached);
        }

        public static void Load(int sceneIndex)
        {
            LazyInit();
            SceneManager.LoadScene(sceneIndex);
        }

        public static void Load(int sceneIndex, LoadSceneMode mode)
        {
            LazyInit();

            if (_instance != null)
                _instance.Load(sceneIndex, mode);
            else
                SceneManager.LoadScene(sceneIndex, mode);
        }

        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void Load(string sceneName, LoadSceneMode mode)
        {
            LazyInit();

            if (_instance != null)
                _instance.Load(SceneUtility.GetBuildIndexByScenePath(sceneName), mode);
            else
                SceneManager.LoadScene(sceneName, mode);
        }

        public static void LoadAsync(int index, LoadSceneMode mode = LoadSceneMode.Single, Action onComplete = null)
        {
            LazyInit();

            if (_instance != null)
            {
                _instance.Load(index, mode, onComplete);
            }
            else
            {
                var operation = SceneManager.LoadSceneAsync(index, mode);
                operation.completed += _ => onComplete?.Invoke();
            }
        }

        public static void LoadAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onComplete = null)
        {
            LazyInit();

            if (_instance != null)
            {
                int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
                _instance.Load(buildIndex, mode, onComplete);
            }
            else
            {
                var operation = SceneManager.LoadSceneAsync(sceneName, mode);
                operation.completed += _ => onComplete?.Invoke();
            }
        }

        public static void Reload()
        {
            LazyInit();

            if (_instance != null)
                _instance.Reload();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
