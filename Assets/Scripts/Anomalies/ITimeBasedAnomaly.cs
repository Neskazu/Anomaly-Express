namespace Anomalies
{
    public interface ITimeBasedAnomaly : IAnomaly
    {
        float Interval { get; set; }
    }
}