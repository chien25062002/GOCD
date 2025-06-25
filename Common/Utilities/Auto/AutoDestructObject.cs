using System;
using GOCD.Framework;
using PrimeTween;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// This class is created for auto destroy or disable gameobject purpose
    /// </summary>
    public class AutoDestructObject : MonoBase
    {
        [Header("Config")]
        [SerializeField] float _delay = 0f;
        [SerializeField] bool _deactiveOnly = false;

        Tween _tween;

        public event Action eventDestruct;

        #region MonoBehaviour

        protected override void OnEnable()
        {
            _tween.Stop();
            _tween = Tween.Delay(_delay, Destruct);
        }

        protected override void OnDisable()
        {
            _tween.Stop();
        }

        #endregion

        void Destruct()
        {
            if (_deactiveOnly)
                GameObjectCached.SetActive(false);
            else
                Destroy(GameObjectCached);

            eventDestruct?.Invoke();
        }
    }
}