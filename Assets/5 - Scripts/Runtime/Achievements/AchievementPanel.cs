using UnityEngine;

namespace Achievements
{
    public class AchievementPanel : MonoBehaviour
    {
        [SerializeField] private AchievementItem itemPrefab;
        [SerializeField] private Transform content;

        private void OnEnable()
        {
            Build();
        }

        private void Build()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (var definition in AchievementManager.Instance.GetAll())
            {
                AchievementProgress progress =
                    AchievementManager.Instance.GetProgress(definition.Id);

                AchievementItem item =
                    Instantiate(itemPrefab, content);

                item.Initialize(definition, progress);
            }
        }
    }
}