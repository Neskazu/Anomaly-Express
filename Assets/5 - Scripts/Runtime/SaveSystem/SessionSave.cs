using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SessionSave
{
    public bool ShowFeedbackPopup;
    public List<string> SeenAnomalies = new List<string>();
    public List<string> SeenMegaAnomalies = new List<string>();
}
