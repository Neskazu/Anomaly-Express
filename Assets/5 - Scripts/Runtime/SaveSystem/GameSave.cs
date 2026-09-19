using System;
using Nac;
using UI;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class GameSave
    {
        [SerializeField] private AchievementSave achievementsSave = new();
        [SerializeField] private SessionSave sessionSave = new();
        [Space]
        [SerializeField] private GeneralSettingsSave generalSettingsSave = new();
        [SerializeField] private GraphicsSettingsSave graphicsSettingsSave = new();
        [SerializeField] private AudioSettingSave audioSettingSave = new();
        [Space]
        [SerializeField] private UiSave uiSave = new();

        public AchievementSave Achievements => achievementsSave;
        public SessionSave Session => sessionSave;

        public GeneralSettingsSave GeneralSettings => generalSettingsSave;
        public GraphicsSettingsSave GraphicsSettings => graphicsSettingsSave;
        public AudioSettingSave AudioSettingSave => audioSettingSave;
        public UiSave Ui => uiSave;
    }
}