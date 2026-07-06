using SaveSystem;
using UnityEngine;

public class MainMenuFeedbackPopup : MonoBehaviour
{
    [SerializeField] private GameObject popup;

    private void Start()
    {
        if (!SaveManager.Save.Session.ShowFeedbackPopup)
            return;

        SaveManager.Save.Session.ShowFeedbackPopup = false;
        SaveManager.SaveGame();

        popup.SetActive(true);
    }
}