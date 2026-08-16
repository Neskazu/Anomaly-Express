using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio
{
    public static class AudioUtility
    {
        public static void Play3D(
            AudioClip clip,
            Vector3 position,
            AudioMixerGroup mixerGroup = null,
            float volume = 1f,
            float pitch = 1f,
            float minDistance = 2f,
            float maxDistance = 15f)
        {
            if (clip == null) return;

            GameObject audioObj = new GameObject($"TempAudio_{clip.name}");
            audioObj.transform.position = position;

            AudioSource source = audioObj.AddComponent<AudioSource>();

            source.clip = clip;
            source.outputAudioMixerGroup = mixerGroup; 

            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;

            source.volume = volume;
            source.pitch = pitch;

            source.Play();

            Object.Destroy(audioObj, clip.length + 0.1f);
        }
    }
}