using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace GOCD.Framework {
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class View : MonoCached {
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

        [FoldoutGroup("Lifecycle")]
        [LabelText("Destroy On Close")]
        [Tooltip("Nếu bật, sau khi Close xong sẽ Destroy(GameObject)")]
        [SerializeField] bool _destroyOnClose = true;

        [FoldoutGroup("Lifecycle")]
        [LabelText("Destroy On Hide")]
        [Tooltip("Nếu bật, sau khi Hide xong sẽ Destroy(GameObject)")]
        [SerializeField] bool _destroyOnHide = false;

        [FoldoutGroup("Lifecycle")]
        [LabelText("Fallback SetInactive On Close")]
        [Tooltip("Nếu KHÔNG destroy khi Close, có set inactive không? (mặc định: không)")]
        [SerializeField] bool _setInactiveOnCloseIfNotDestroy = false;

        CancellationTokenSource _cancelToken;
        Sequence _sequence;
        CanvasGroup _canvasGroup;

        public Sequence sequence => _sequence;

        public UnityEvent onOpenStart => _onOpenStart;
        public UnityEvent onOpenEnd   => _onOpenEnd;
        public UnityEvent onCloseStart => _onCloseStart;
        public UnityEvent onCloseEnd   => _onCloseEnd;
        public UnityEvent onShowStart => _onShowStart;
        public UnityEvent onShowEnd   => _onShowEnd;
        public UnityEvent onHideStart => _onHideStart;
        public UnityEvent onHideEnd   => _onHideEnd;

        public bool interactable { get => _canvasGroup.interactable; set => _canvasGroup.interactable = value; }
        public bool hideOnBlock => _hideOnBlock;

        #region MonoBehaviour
        void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        void OnDestroy() {
            // Cancel token
            _cancelToken?.Cancel();
            _cancelToken?.Dispose();

            // Kill tweens
            _sequence?.Kill();
        }
        #endregion

        #region Private
        [FoldoutGroup("Transition")]
        [Button]
        void GetTransitionEntities() {
            _transitionEntities = GetComponentsInChildren<ViewTransitionEntity>(true);
        }

        void ConstructSequence() {
            if (_sequence != null)
                return;

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            if ((_extras == null || _extras.Length == 0) && (_transitionEntities == null || _transitionEntities.Length == 0)) {
                _sequence.AppendInterval(1.0f);
            } else {
                if (_extras != null) {
                    for (int i = 0; i < _extras.Length; i++)
                        _extras[i]?.Apply(this);
                }

                if (_transitionEntities != null) {
                    for (int i = 0; i < _transitionEntities.Length; i++)
                        _transitionEntities[i]?.Apply(this);
                }
            }

            _sequence.SetUpdate(true);
            _sequence.SetAutoKill(false);
        }

        async UniTask ProcessOpen(bool isShow) {
            // Handle cancel token
            _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();

            ConstructSequence();

            // Open start callback
            if (isShow) _onShowStart?.Invoke();
            else        _onOpenStart?.Invoke();

            // Active object when open (in case it hidden before)
            GameObjectCached.SetActive(true);

            _canvasGroup.interactable = false;

            if (_openDuration > 0.0f) {
                _sequence.timeScale = _openDuration > 0.0f ? 1.0f / _openDuration : 1.0f;

                _sequence.Complete();
                _sequence.Restart();
                _sequence.Play();

                await UniTask.WaitForSeconds(_openDuration, true, cancellationToken: _cancelToken.Token);
            } else {
                _sequence.Complete();
            }

            // Open end callback
            if (isShow) _onShowEnd?.Invoke();
            else        _onOpenEnd?.Invoke();

            _canvasGroup.interactable = true;
        }

        async UniTask ProcessClose(bool isHiding) {
            // Handle cancel token
            _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();

            ConstructSequence();

            // Close start callback
            if (isHiding) _onHideStart?.Invoke();
            else          _onCloseStart?.Invoke();

            _canvasGroup.interactable = false;

            if (_closeDuration > 0.0f) {
                _sequence.timeScale = _closeDuration > 0.0f ? 1.0f / _closeDuration : 1.0f;

                _sequence.Complete();
                _sequence.PlayBackwards();

                await UniTask.WaitForSeconds(_closeDuration, true, cancellationToken: _cancelToken.Token);
            } else {
                _sequence.Rewind();
            }

            if (isHiding) {
                _onHideEnd?.Invoke();

                if (_destroyOnHide) {
                    // Huỷ hẳn object sau khi hide
                    Destroy(gameObject);
                    return;
                } else {
                    GameObjectCached.SetActive(false);
                }
            } else {
                _onCloseEnd?.Invoke();

                if (_destroyOnClose) {
                    // Huỷ hẳn object sau khi close
                    Destroy(gameObject);
                    return;
                } else if (_setInactiveOnCloseIfNotDestroy) {
                    // Tuỳ chọn: nếu không destroy khi close thì tắt GameObject
                    GameObjectCached.SetActive(false);
                }
            }
        }
        #endregion

        #region Public API
        public void Open() {
            ProcessOpen(false).Forget();
        }

        public void Close() {
            ProcessClose(false).Forget();
        }

        public void Reveal() {
            _canvasGroup.interactable = true;
            if (_hideOnBlock)
                ProcessOpen(true).Forget();
        }

        public void Block() {
            _canvasGroup.interactable = false;
            if (_hideOnBlock)
                ProcessClose(true).Forget();
        }
        #endregion
    }
}
