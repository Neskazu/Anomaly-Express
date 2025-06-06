using UnityEngine;

public interface ITimeBasedAnomaly : IAnomaly
{
    float Interval { get; set; }
}
