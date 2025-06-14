namespace Anomalies
{
    public interface ITriggerableByPlayer : IAnomaly
    {
        void OnPlayerEnterZone();

        void OnPlayerExitZone();
    }
}