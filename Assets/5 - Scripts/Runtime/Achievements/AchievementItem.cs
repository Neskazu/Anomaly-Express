using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Localization;

namespace Achievements
{
    public class AchievementItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image lockIcon;

        private AchievementDefinition definition;
        private AchievementProgress progress;

        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged += Refresh;
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged -= Refresh;
        }

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
                titleText.text = "???";
                descriptionText.text = "???";
                icon.sprite = null;
            }
            else
            {
                icon.sprite = definition.Icon;

                titleText.text = LocalizationManager.Instance.Get(definition.TitleKey);
                descriptionText.text = LocalizationManager.Instance.Get(definition.DescriptionKey);
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

            lockIcon.enabled = !progress.Unlocked;
        }
    }
}