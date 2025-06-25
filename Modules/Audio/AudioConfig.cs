using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework.Audio
{
    [System.Serializable]
    public class AudioConfig : ScriptableObject
    {
        [SerializeField] AudioClip _clip;
        [SerializeField] AudioType _type;
        [SerializeField] bool _is3D;

        [ShowIf("@_is3D")]
        [MinMaxSlider(0f, 200f, ShowFields = true)]
        [SerializeField] Vector2 _distance = new Vector2(1f, 10f);

        [Range(0f, 1f)]
        [SerializeField] float _volumeScale = 1f;

        [SerializeField] AudioGroupType _group;

        public AudioClip Clip => _clip;
        public AudioType Type => _type;
        public bool Is3D => _is3D;
        public Vector2 Distance => _distance;
        public float VolumeScale => _volumeScale;

        public AudioGroupType Group => _group;

        public void Construct(AudioClip clip)
        {
            _clip = clip;
        }
    }
}