namespace Anomalies
{
    public class ToggleSensorComponent : SensorComponent
    {
        public void Set(bool value)
        {
            detected.Value = value;
        }
    }
}