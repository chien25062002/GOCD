using CodeSketch.Diagnostics;
using UnityEngine;
using PrimeTween;

using CodeSketch.Mono;

namespace CodeSketch.Audio
{
    public class AudioScript : MonoBase
    {
        #region Properties

        AudioConfig _config;
        AudioSource _audioSource;

        Tween _tweendelay;

        public AudioConfig Config => _config;

        public AudioSource AudioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = GameObjectCached.GetComponent<AudioSource>();

                return _audioSource;
            }
        }

        #endregion

        #region MonoBehaviour

        void Awake()
        {
            _audioSource = AudioSource;
        }

        void OnDestroy()
        {
            _tweendelay.Stop();
        }

        protected override void Start()
        {
            base.Start();
            
            AudioManager.Attach(TransformCached);

            AudioManager.VolumeSound.OnValueChanged += VolumeSound_EventValueChanged;
            AudioManager.VolumnMusic.OnValueChanged += VolumeMusic_EventValueChanged;
        }

        #endregion

        #region Function -> Public

        public void Play(AudioConfig config, bool loop = false)
        {
            Init(config, loop);

            _tweendelay.Stop();

            if (!loop)
                _tweendelay = Tween.Delay(config.Clip.length, Stop);
        }

        public void Stop()
        {
            if (!AudioManager.HasInstance)
                return;

            _tweendelay.Stop();
            AudioSource.Stop();

            AudioManager.ReturnPool(this);
        }

        public void TryStop(AudioConfig config)
        {
            if (!config) return;
            TryStop(config.Clip);
        }

        public void TryStop(AudioClip clip)
        {
            if (clip == null || !AudioSource.isPlaying) return;
            
            if (AudioSource.clip.name.Equals(clip.name))
                Stop();
        }
        
        #endregion

        #region Function -> Private

        float GetVolume()
        {
            return _config.Volume * (_config.Type == AudioType.Music ? AudioManager.VolumnMusic.Value : AudioManager.VolumeSound.Value);
        }
        
        void UpdateVolume()
        {
            if (_config == null) return;

            float volume = GetVolume();
            AudioSource.mute = volume <= 0;
            AudioSource.volume = volume;
            
            Tween.AudioVolume(AudioSource, volume, 0.1f);
        }

        void VolumeSound_EventValueChanged(float volume)
        {
            UpdateVolume();
        }

        void VolumeMusic_EventValueChanged(float volume)
        {
            UpdateVolume();
        }
        
        void Init(AudioConfig config, bool loop = false)
        {
            if (config == null || config.Clip == null)
            {
                CodeSketchDebug.LogWarning("[AudioScript] Invalid config or clip!");
                return;
            }
            
            if (_audioSource == null)
                _audioSource = AudioSource;
            if (AudioSource == null) return;

            _config = config;

            AudioSource.clip = config.Clip;
            AudioSource.loop = loop;
            AudioSource.minDistance = config.EarsDistance.x;
            AudioSource.maxDistance = config.EarsDistance.y;
            AudioSource.spatialBlend = config.Mode == AudioMode.Mode3D ? 1f : 0f;
            AudioSource.rolloffMode = AudioRolloffMode.Linear;

            AudioSource.outputAudioMixerGroup = config.Bus == AudioBus.Master ? null : AudioMixerFactory.GetGroup(config.Bus);

            UpdateVolume();
            AudioSource.Play();
        }

        #endregion
    }
    
    public static class AudioExtensions 
    {
        public static void TryStop(this AudioScript audio, AudioConfig config)
        {
            if (audio == null) return;
            audio.TryStop(config);
        }

        public static bool IsClip(this AudioScript audio, AudioConfig config)
        {
            if (audio == null || config == null || audio.AudioSource.clip == null) return false;
            if (audio.AudioSource.clip.name.Equals(config.Clip.name)) return true;
            return false;
        }
    }
}