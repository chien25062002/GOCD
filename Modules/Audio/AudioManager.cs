using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using GOCD.Framework.Diagnostics; // Import để sử dụng HashSet

namespace GOCD.Framework.Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        public static GOCDValue<float> volumeMusic = new GOCDValue<float>(1.0f);
        public static GOCDValue<float> volumeSound = new GOCDValue<float>(1.0f);

        ObjectPool<AudioScript> _pool;
        HashSet<AudioScript> _activeAudioScripts = new HashSet<AudioScript>(); // Lưu trữ các AudioScript đang active

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();
            InitPool();
        }

        #endregion

        #region Function -> Public

        public static AudioScript Play(AudioConfig config, bool loop = false)
        {
            if (config == null || config.Clip == null)
                return null;

            AudioScript audio = Instance._pool.Get();
            audio.Play(config, loop);

            // Thêm vào danh sách đang sử dụng
            Instance._activeAudioScripts.Add(audio);

            return audio;
        }
        
        public static AudioScript Play(AudioConfig config, Vector3 position, bool loop = false)
        {
            if (config == null || config.Clip == null)
                return null;

            AudioScript audio = Instance._pool.Get();
            audio.TransformCached.position = position;
            audio.Play(config, loop);

            Instance._activeAudioScripts.Add(audio);

            return audio;
        }

        public static void ReturnPool(AudioScript audioScript)
        {
            if (audioScript == null) return;

            // Kiểm tra xem object có đang trong danh sách active không
            if (Instance._activeAudioScripts.Contains(audioScript))
            {
                Instance._activeAudioScripts.Remove(audioScript);
                Instance._pool.Release(audioScript);
            }
            else
            {
                GOCDDebug.LogWarning($"[AudioManager] Trying to release an object that has already been released: {audioScript.name}");
            }
        }

        #endregion

        #region Function -> Private

        void InitPool()
        {
            _pool = new ObjectPool<AudioScript>(
                createFunc: () =>
                {
                    GameObject obj = new GameObject(typeof(AudioScript).ToString(), typeof(AudioSource));
                    return obj.AddComponent<AudioScript>();
                },
                actionOnGet: _audio =>
                {
                    _audio.GameObjectCached.SetActive(true);
                },
                actionOnRelease: (_audio) =>
                {
                    _audio.GameObjectCached.SetActive(false);
                },
                actionOnDestroy: (_audio) =>
                {
                    Destroy(_audio.gameObject);
                },
                collectionCheck: true, // Kiểm tra lỗi nếu object đã được release trước đó (Unity 2022+)
                defaultCapacity: 10,
                maxSize: 50
            );
        }

        #endregion
    }
}
