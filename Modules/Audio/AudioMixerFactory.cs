using System;
using System.Collections.Generic;
using GOCD.Framework.Diagnostics;
using UnityEngine;
using UnityEngine.Audio;

namespace GOCD.Framework.Audio
{
    public class AudioMixerFactory : ScriptableObjectSingleton<AudioMixerFactory>
    {
        [System.Serializable]
        public class GroupMapping
        {
            public AudioGroupType type;
            public AudioMixerGroup group;
        }

        [SerializeField] List<GroupMapping> _mappings = new();

        [NonSerialized] Dictionary<AudioGroupType, AudioMixerGroup> _lookup;

        public static AudioMixerGroup GetGroup(AudioGroupType type)
        {
            if (Instance._lookup.TryGetValue(type, out var group))
                return group;

            GOCDDebug.Log($"AudioMixer group not found for type: {type}", Color.yellow);
            return null;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _lookup = new Dictionary<AudioGroupType, AudioMixerGroup>();
            foreach (var entry in _mappings)
            {
                _lookup.TryAdd(entry.type, entry.group);
            }
        }
    }
}
