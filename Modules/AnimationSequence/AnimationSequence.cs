﻿using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GOCD.Framework
{
    public class AnimationSequence : MonoBase
    {
        [Serializable, Flags]
        public enum Callback
        {
            OnStart = 1 << 1,
            OnPlay = 1 << 2,
            OnUpdate = 1 << 3,
            OnStep = 1 << 4,
            OnComplete = 1 << 5,
            OnRewind = 1 << 6,
        }

        [Serializable, Flags]
        public enum ActionOnEnable
        {
            Restart = 1 << 1,
            Complete = 1 << 2,
            PlayFoward = 1 << 3,
            PlayBackwards = 1 << 4,
        }

        [Serializable, Flags]
        public enum ActionOnDisable
        {
            Kill = 1 << 1,
            Pause = 1 << 2,
        }

        [Title("Steps")]
        [ListDrawerSettings(ShowIndexLabels = false, OnBeginListElementGUI = "BeginDrawListElement", OnEndListElementGUI = "EndDrawListElement", AddCopiesLastElement = true)]
        [SerializeReference] AnimationSequenceStep[] _steps = new AnimationSequenceStep[0];

        [Title("Settings")]
        [SerializeField] bool _isAutoKill = true;

        [SerializeField] ActionOnEnable _actionOnEnable;

        [SerializeField] ActionOnDisable _actionOnDisable;

        [MinValue(-1), HorizontalGroup("Loop")]
        [SerializeField] int _loopCount;

        [ShowIf("@_loopCount != 0"), HorizontalGroup("Loop"), LabelWidth(75.0f)]
        [SerializeField] LoopType _loopType;

        [HorizontalGroup("Update")]
        [InlineButton("@_isIndependentUpdate = true", Label = "Timescale Based", ShowIf = ("@_isIndependentUpdate == false"))]
        [InlineButton("@_isIndependentUpdate = false", Label = "Independent Update", ShowIf = ("@_isIndependentUpdate == true"))]
        [SerializeField] protected UpdateType _updateType = UpdateType.Normal;

        [HideInInspector]
        [SerializeField] protected bool _isIndependentUpdate = false;

        [SerializeField] float _delay;

        Sequence _sequence;

        RectTransform _rectTransform;

        Graphic _graphic;

        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        public Graphic graphic
        {
            get
            {
                if (_graphic == null)
                    _graphic = GetComponent<Graphic>();

                return _graphic;
            }
        }

        public Sequence sequence
        {
            get
            {
                InitSequence();

                return _sequence;
            }
        }

        void OnDestroy()
        {
            _sequence?.Kill();
        }

        void OnEnable()
        {
            // If no action flagged on enable, we shouldn't init sequence
            if(_actionOnEnable != 0)
                InitSequence();

            if (_actionOnEnable.HasFlag(ActionOnEnable.Complete))
                _sequence?.Complete();

            if (_actionOnEnable.HasFlag(ActionOnEnable.Restart))
                _sequence?.Restart();

            if (_actionOnEnable.HasFlag(ActionOnEnable.PlayFoward))
                _sequence?.PlayForward();

            if (_actionOnEnable.HasFlag(ActionOnEnable.PlayBackwards))
                _sequence?.PlayBackwards();
        }

        void OnDisable()
        {
            if (_actionOnDisable.HasFlag(ActionOnDisable.Pause))
                _sequence?.Pause();

            if (_actionOnDisable.HasFlag(ActionOnDisable.Kill))
                _sequence?.Kill();
        }

        public void InitSequence()
        {
            if (_sequence.IsActive())
                return;

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            for (int i = 0; i < _steps.Length; i++)
            {
                _steps[i].AddToSequence(this);
            }

            _sequence.SetAutoKill(_isAutoKill);

            _sequence.SetLoops(_loopCount, _loopType);

            _sequence.SetDelay(_delay);

            _sequence.SetUpdate(_updateType, _isIndependentUpdate);
        }

        [ButtonGroup(Order = -1, ButtonHeight = 25)]
        [Button(Name = "", Icon = SdfIconType.PlayFill)]
        public void Play()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _sequence?.Restart();

                InitSequence();

                DG.DOTweenEditor.DOTweenEditorPreview.PrepareTweenForPreview(_sequence);
                DG.DOTweenEditor.DOTweenEditorPreview.Start();

                return;
            }
#endif
            _sequence?.Play();
        }

        public void Restart()
        {
            _sequence?.Restart();
        }

        public void PlayForward()
        {
            _sequence?.PlayForward();
        }

        public void PlayBackwards()
        {
            _sequence?.PlayBackwards();
        }

        public void Pause()
        {
            _sequence?.Pause();
        }
        
#if UNITY_EDITOR

        [ButtonGroup]
        [Button(Name = "", Icon = SdfIconType.SkipStartFill)]
        void PlayBackward()
        {
            _sequence?.PlayBackwards();
        }

        [ButtonGroup]
        [Button(Name = "", Icon = SdfIconType.SkipEndFill)]
        void PlayFoward()
        {
            _sequence?.PlayForward();
        }

        [ButtonGroup]
        [Button(Name = "", Icon = SdfIconType.StopFill)]
        void Stop()
        {
            DG.DOTweenEditor.DOTweenEditorPreview.Stop(true);

            _sequence?.Kill();
            _sequence = null;
        }

        [ButtonGroup]
        [Button(Name = "", Icon = SdfIconType.SkipBackwardFill)]
        void Rewind()
        {
            _sequence?.Rewind();
        }

        [ButtonGroup]
        [Button(Name = "", Icon = SdfIconType.SkipForwardFill)]
        void Complete()
        {
            _sequence?.Complete();
        }

        void BeginDrawListElement(int index)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(_steps[index].displayName);
        }

        void EndDrawListElement(int index)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
        }

#endif
    }
}