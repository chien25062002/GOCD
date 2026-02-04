using System;
using System.Collections.Generic;
using CodeSketch.Diagnostics;
using UnityEngine;
using UnityEngine.Audio;

using CodeSketch.SO;

namespace CodeSketch.Audio
{
    public class AudioMixerFactory : ScriptableObjectSingleton<AudioMixerFactory>
    {
        [System.Serializable]
        public class BusMapper
        {
            public AudioBus Bus;
            public AudioMixerGroup MixerGroup;
        }
        
        [SerializeField] List<BusMapper> _mappers = new();

        [SerializeField] AudioConfig _sfxUIButtonClick;

        [NonSerialized] Dictionary<AudioBus, AudioMixerGroup> _lookup;

        public static AudioConfig SfxUIButtonClick => Instance._sfxUIButtonClick;

        public static AudioMixerGroup GetGroup(AudioBus bus)
        {
            if (bus == AudioBus.Master || bus == AudioBus.None)
                return null;

            if (Instance._lookup.TryGetValue(bus, out var group))
                return group;

            CodeSketchDebug.LogWarning($"Mixer group not found for bus: {bus}");
            return null;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _lookup = new Dictionary<AudioBus, AudioMixerGroup>();
            foreach (var entry in _mappers)
            {
                _lookup.TryAdd(entry.Bus, entry.MixerGroup);
            }
        }
    }
}
