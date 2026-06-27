using Achievements;
using System;
using System.Collections.Generic;

namespace SaveSystem
{
    [Serializable]
    public class AchievementSave
    {
        public List<AchievementProgress> Progress = new();
    }
}