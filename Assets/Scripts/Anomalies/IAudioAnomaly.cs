namespace Anomalies
{
    public interface IAudioAnomaly : IAnomaly
    {
        public const float Mute = -80;
        public const float Unmute = 0;
    }
}