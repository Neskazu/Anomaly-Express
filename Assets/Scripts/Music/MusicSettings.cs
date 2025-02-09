using UnityEngine;

namespace Music
{
    [CreateAssetMenu(fileName = "MusicSettings", menuName = "Scriptable Objects/MusicSettings")]
    public class MusicSettings : ScriptableObject
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }
}
