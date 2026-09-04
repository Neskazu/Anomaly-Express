using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Localization;

namespace Achievements
{
    public class AchievementItem : MonoBehaviour
    {
        [SerializeField] private Image icon;

        [SerializeField] private LocalizedText titleText;
        [SerializeField] private LocalizedText descriptionText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Image unlockIcon;

        [SerializeField] private string hiddenTitleKey = "hidden_achievement_title";
        [SerializeField] private string hiddenDescKey = "hidden_achievement_desc";

        private AchievementDefinition definition;
        private AchievementProgress progress;

        public void Initialize(AchievementDefinition definition, AchievementProgress progress)
        {
            this.definition = definition;
            this.progress = progress;

            Refresh();
        }

        public void Refresh()
        {
            bool hidden = definition.Hidden && !progress.Unlocked;

            if (hidden)
            {
                titleText.Key = hiddenTitleKey;
                descriptionText.Key = hiddenDescKey;
                icon.sprite = null;
            }
            else
            {
                titleText.Key = definition.TitleKey;
                descriptionText.Key = definition.DescriptionKey;
                icon.sprite = definition.Icon;
            }

            if (definition.MaxProgress > 1)
            {
                progressText.gameObject.SetActive(true);
                progressText.text = $"{progress.CurrentProgress}/{definition.MaxProgress}";
            }
            else
            {
                progressText.gameObject.SetActive(false);
            }

            lockIcon.gameObject.SetActive(!progress.Unlocked);
            unlockIcon.gameObject.SetActive(progress.Unlocked);
        }
    }
}