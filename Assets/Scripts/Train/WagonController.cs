using UnityEngine;

public class WagonController : MonoBehaviour
{
    public bool hasAnomaly;
    public Transform ForwardCenter;
    public Transform BackwardCenter;

    public Vector3 GetOffest(VestibuleType vestibuleType)
    {
        if (vestibuleType == VestibuleType.Forward)
        {
            return ForwardCenter.position;
        }
        else
        {
            return BackwardCenter.position;
        }
    }
    public Vector3 GetReversedOffset(VestibuleType vestibule)
    {
        if (vestibule == VestibuleType.Forward)
        {
            return BackwardCenter.position;
        }
        else
        {
            return ForwardCenter.position;
        }
    }
}
