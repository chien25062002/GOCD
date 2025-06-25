using DG.Tweening;
using GOCD.Framework.Diagnostics;
using UnityEngine;

namespace GOCD.Framework.Audio
{
    public class AudioScript : MonoCached
    {
        AudioConfig _config;

        AudioSource _audioSource;

        Tween _tween;

        public AudioConfig config => _config;

        public AudioSource audioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GameObjectCached.GetComponent<AudioSource>();

                return _audioSource;
            }
        }

        #region MonoBehaviour

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        void OnDestroy()
        {
            _tween?.Kill();
        }

        void Start()
        {
            TransformCached.SetParent(AudioManager.Instance.TransformCached);

            AudioManager.volumeSound.OnValueChanged += VolumeSound_EventValueChanged;
            AudioManager.volumeMusic.OnValueChanged += VolumeMusic_EventValueChanged;
        }

        #endregion

        #region Function -> Public

        public void Play(AudioConfig config, bool loop = false)
        {
            Construct(config, loop);

            _tween?.Kill();

            if (!loop)
                _tween = DOVirtual.DelayedCall(config.Clip.length, Stop, false);
        }

        public void Stop()
        {
            if (AudioManager.IsDestroyed)
                return;

            _tween?.Kill();

            audioSource.Stop();

            AudioManager.ReturnPool(this);
        }

        public void StopIf(AudioClip clip)
        {
            if (_config == null) return;
            if (_config.Clip.name.Equals(clip.name) && audioSource.isPlaying)
                Stop();
        }
        
        public void StopIf(AudioConfig config)
        {
            if (_config == null) return;
            if (_config.Clip.name.Equals(config.Clip.name) && audioSource.isPlaying)
                Stop();
        }
        
        #endregion

        #region Function -> Private

        float GetVolume()
        {
            return _config.VolumeScale * (_config.Type == AudioType.Music ? AudioManager.volumeMusic.value : AudioManager.volumeSound.value);
        }

        void VolumeSound_EventValueChanged(float volume)
        {
            UpdateVolume();
        }

        void VolumeMusic_EventValueChanged(float volume)
        {
            UpdateVolume();
        }
        
        void Construct(AudioConfig config, bool loop = false)
        {
            if (config == null || config.Clip == null)
            {
                GOCDDebug.LogWarning("[AudioScript] Invalid config or clip!");
                return;
            }

            _config = config;

            if (audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            audioSource.clip = config.Clip;
            audioSource.loop = loop;
            audioSource.minDistance = config.Distance.x;
            audioSource.maxDistance = config.Distance.y;
            audioSource.spatialBlend = config.Is3D ? 1f : 0f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            audioSource.outputAudioMixerGroup = config.Group == AudioGroupType.None ? null : AudioMixerFactory.GetGroup(config.Group);

            UpdateVolume();
            audioSource.Play();
        }

        void UpdateVolume()
        {
            if (_config == null)
                return;

            float volumeFinal = GetVolume();

            audioSource.mute = volumeFinal <= 0;
            audioSource.volume = volumeFinal;
            
            // audioSource.DOFade(volumeFinal, 0.1f);
        }

        #endregion
    }
    
    public static class AudioScriptExtensions 
    {
        public static void StopIfClip(this AudioScript asScript, AudioConfig config)
        {
            if (asScript == null) return;
            
            asScript.StopIf(config);
        }

        public static bool IsClip(this AudioScript asScript, AudioConfig config)
        {
            if (asScript == null) return false;
            
            if (asScript.config == null) return false;
            if (asScript.config.Clip.name.Equals(config.Clip.name)) return true;
            return false;
        }
    }
}