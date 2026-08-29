using SaveSystem;
using UnityEngine;

public class ProgressReset : MonoBehaviour
{
    void Start()
    {
        SaveManager.Save.Session.CompletedMegasThisRun = 0;
        SaveManager.SaveGame();
    }
}