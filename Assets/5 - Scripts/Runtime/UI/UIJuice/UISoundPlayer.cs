using UnityEngine;
using UnityEngine.Audio;

public class UISoundPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource source;

    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip secretClip;

    [Header("Pitch")]
    [SerializeField] private bool randomPitch = true;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.96f, 1.04f);

    public void PlayClick() => Play(clickClip, true);
    public void PlayHover() => Play(hoverClip, true);
    public void PlaySecret() => Play(secretClip, false);

    private void Play(AudioClip clip, bool usePitch)
    {
        if (source == null || clip == null)
            return;

        source.pitch = usePitch && randomPitch
            ? Random.Range(pitchRange.x, pitchRange.y)
            : 1f;

        source.PlayOneShot(clip);
    }

    public void Stop()
    {
        if (source == null)
            return;

        source.Stop();
    }
}