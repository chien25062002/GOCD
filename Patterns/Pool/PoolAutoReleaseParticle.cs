using System;
using CodeSketch.Mono;
using PrimeTween;
using UnityEngine;

namespace CodeSketch.Patterns.Pool
{
    public class PoolAutoReleaseParticle : MonoBase
    {
        [SerializeField] ParticleSystem _particle;
        [SerializeField] PoolPrefabConfig _pool;

        Tween _tween;

        public Action OnCompleted;

        void OnDestroy()
        {
            OnCompleted?.Invoke();
            _tween.Stop();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnCompleted?.Invoke();
        }

        protected override void OnEnable()
        {
             base.OnEnable();
             
            ParticleSystem.MainModule particleMain = _particle.main;
            particleMain.playOnAwake = false;
            
            _particle.Play();
            _tween.Stop();

            float duration = _particle != null ? _particle.main.duration : 0f;
            _tween = Tween.Delay(duration, OnDestructable);
        }

        void OnDestructable()
        {
            if (_pool)
            {
                PoolPrefabGlobal.Release(_pool, GameObjectCached);
            }
            else
            {
                Destroy(GameObjectCached);
            }
        }

        void OnValidate()
        {
            if (_particle == null)
                _particle = GetComponent<ParticleSystem>();
            if (_particle == null)
                _particle = GetComponentInChildren<ParticleSystem>();
        }
    }
}
