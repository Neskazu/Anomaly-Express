using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SessionSave
{
    public bool ShowFeedbackPopup;
    public List<int> SeenAnomalies = new List<int>();
}
