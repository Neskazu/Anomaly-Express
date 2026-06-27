using System;


namespace SaveSystem
{
    [Serializable]
    public class GameSave
    {
        public AchievementSave Achievements = new();
        public SettingsSave Settings = new();
    }
}
