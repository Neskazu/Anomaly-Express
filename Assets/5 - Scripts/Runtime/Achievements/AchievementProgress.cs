using System;
namespace Achievements
{
    [Serializable]
    public class AchievementProgress
    {
        public string Id;

        public bool Unlocked;

        public int CurrentProgress;

        public long UnlockTimestamp;
    }
}