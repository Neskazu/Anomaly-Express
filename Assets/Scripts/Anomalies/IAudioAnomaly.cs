using UnityEngine;

public interface IAudioAnomaly : IAnomaly
{
    AudioClip Clip { get; set; }
    float Volume { get; set; }
}
