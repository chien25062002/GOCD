using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace GOCD.Framework
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class View : MonoCached
    {
        [Title("Config")]
        [MinValue(0f)]
        [SerializeField] float _openDuration = 0.1f;

        [MinValue(0f)]
        [SerializeField] float _closeDuration = 0.1f;

        [FoldoutGroup("Extra", Expanded = false)]
        [ListDrawerSettings(ListElementLabelName = "displayName")]
        [SerializeReference] ViewExtra[] _extras = Array.Empty<ViewExtra>();

        [FoldoutGroup("Extra")]
        [SerializeField] bool _hideOnBlock = false;

        [FoldoutGroup("Transition", Expanded = false)]
        [SerializeField] ViewTransitionEntity[] _transitionEntities;

        [FoldoutGroup("Event", Expanded = false)]
        [SerializeField] UnityEvent _onOpenStart;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onOpenEnd;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onCloseStart;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onCloseEnd;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onShowStart;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onShowEnd;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onHideStart;
        [FoldoutGroup("Event")]
        [SerializeField] UnityEvent _onHideEnd;

        CancellationTokenSource _cancelToken;

        Sequence _sequence;

        CanvasGroup _canvasGroup;

        public Sequence sequence { get { return _sequence; } }

        public UnityEvent onOpenStart { get { return _onOpenStart; } }
        public UnityEvent onOpenEnd { get { return _onOpenEnd; } }
        public UnityEvent onCloseStart { get { return _onCloseStart; } }
        public UnityEvent onCloseEnd { get { return _onCloseEnd; } }
        public UnityEvent onShowStart { get { return _onShowStart; } }
        public UnityEvent onShowEnd { get { return _onShowEnd; } }
        public UnityEvent onHideStart { get { return _onHideStart; } }
        public UnityEvent onHideEnd { get { return _onHideEnd; } }

        public bool interactable { get { return _canvasGroup.interactable; } set { _canvasGroup.interactable = value; } }

        public bool hideOnBlock { get { return _hideOnBlock; } }

        #region MonoBehaviour

        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void OnDestroy()
        {
            // Cancel token
            _cancelToken?.Cancel();
            _cancelToken?.Dispose();

            // Kill tweens
            _sequence?.Kill();
        }

        #endregion

        #region Function -> Private

        [FoldoutGroup("Transition")]
        [Button]
        void GetTransitionEntities()
        {
            _transitionEntities = GetComponentsInChildren<ViewTransitionEntity>(true);
        }

        void ConstructSequence()
        {
            if (_sequence != null)
                return;

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            if (_extras.IsNullOrEmpty() && _transitionEntities.IsNullOrEmpty())
            {
                _sequence.AppendInterval(1.0f);
            }
            else
            {
                for (int i = 0; i < _extras.Length; i++)
                    _extras[i].Apply(this);

                for (int i = 0; i < _transitionEntities.Length; i++)
                    _transitionEntities[i].Apply(this);
            }

            _sequence.SetUpdate(true);
            _sequence.SetAutoKill(false);
        }

        async UniTask ProcessOpen(bool isShow)
        {
            // Handle cancel token
            _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();

            ConstructSequence();

            // Open start callback
            if (isShow)
                _onShowStart?.Invoke();
            else
                _onOpenStart?.Invoke();

            // Active object when open (in case it hidden before)
            GameObjectCached.SetActive(true);

            _canvasGroup.interactable = false;

            if (_openDuration > 0.0f)
            {
                _sequence.timeScale = _openDuration > 0.0f ? 1.0f / _openDuration : 1.0f;

                _sequence.Complete();
                _sequence.Restart();
                _sequence.Play();

                await UniTask.WaitForSeconds(_openDuration, true, cancellationToken: _cancelToken.Token);
            }
            else
            {
                _sequence.Complete();
            }

            // Open end callback
            if (isShow)
                _onShowEnd?.Invoke();
            else
                _onOpenEnd?.Invoke();

            _canvasGroup.interactable = true;
        }

        async UniTask ProcessClose(bool isHiding)
        {
            // Handle cancel token
            _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();

            ConstructSequence();

            // Close start callback
            if (isHiding)
                _onHideStart?.Invoke();
            else
                _onCloseStart?.Invoke();

            _canvasGroup.interactable = false;

            if (_closeDuration > 0.0f)
            {
                _sequence.timeScale = _closeDuration > 0.0f ? 1.0f / _closeDuration : 1.0f;

                _sequence.Complete();
                _sequence.PlayBackwards();

                await UniTask.WaitForSeconds(_closeDuration, true, cancellationToken: _cancelToken.Token);
            }
            else
            {
                _sequence.Rewind();
            }

            if (isHiding)
            {
                _onHideEnd?.Invoke();
                GameObjectCached.SetActive(false);
            }
            else
            {
                _onCloseEnd?.Invoke();
            }
        }

        #endregion

        #region Function -> Public

        public void Open()
        {
            ProcessOpen(false).Forget();
        }

        public void Close()
        {
            ProcessClose(false).Forget();
        }

        public void Reveal()
        {
            _canvasGroup.interactable = true;

            if (_hideOnBlock)
                ProcessOpen(true).Forget();
        }

        public void Block()
        {
            _canvasGroup.interactable = false;

            if (_hideOnBlock)
                ProcessClose(true).Forget();
        }

        #endregion
    }
}