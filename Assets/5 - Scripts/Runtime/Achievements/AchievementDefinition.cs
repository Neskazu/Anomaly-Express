using UnityEngine;

namespace Achievements
{
    [CreateAssetMenu(menuName = "Achievements/Achievement")]
    public class AchievementDefinition : ScriptableObject
    {
        public string Id;

        public string TitleKey;
        public string DescriptionKey;

        public Sprite Icon;

        public bool Hidden;

        [Min(1)]
        public int MaxProgress = 1;
    }
}