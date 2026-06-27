using SaveSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance;

        [SerializeField]
        private List<AchievementDefinition> achievements;

        private readonly Dictionary<string, AchievementDefinition> definitions = new();
        private readonly Dictionary<string, AchievementProgress> progress = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            foreach (var achievement in achievements)
            {
                definitions[achievement.Id] = achievement;
            }

            LoadProgress();
        }
        private void LoadProgress()
        {
            progress.Clear();

            foreach (var achievement in SaveManager.Save.Achievements.Progress)
            {
                progress[achievement.Id] = achievement;
            }

            foreach (var definition in achievements)
            {
                if (!progress.ContainsKey(definition.Id))
                {
                    var newProgress = new AchievementProgress
                    {
                        Id = definition.Id
                    };

                    progress[definition.Id] = newProgress;
                    SaveManager.Save.Achievements.Progress.Add(newProgress);
                }
            }

            SaveManager.SaveGame();
        }

        public void Unlock(string id)
        {
            if (!progress.TryGetValue(id, out var achievementProgress))
                return;

            if (achievementProgress.Unlocked)
                return;

            achievementProgress.Unlocked = true;
            achievementProgress.CurrentProgress = definitions[id].MaxProgress;
            achievementProgress.UnlockTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            SaveProgress();

            Debug.Log($"Achievement unlocked: {id}");
        }

        public void AddProgress(string id, int amount = 1)
        {
            if (!progress.TryGetValue(id, out var achievementProgress))
                return;

            if (achievementProgress.Unlocked)
                return;

            achievementProgress.CurrentProgress += amount;

            var definition = definitions[id];

            if (achievementProgress.CurrentProgress >= definition.MaxProgress)
            {
                Unlock(id);
            }
            else
            {
                SaveProgress();
            }
        }

        public void SetProgress(string id, int value)
        {
            if (!progress.TryGetValue(id, out var achievementProgress))
                return;

            achievementProgress.CurrentProgress = value;

            var definition = definitions[id];

            if (achievementProgress.CurrentProgress >= definition.MaxProgress)
            {
                Unlock(id);
            }
            else
            {
                SaveProgress();
            }
        }
        private void SaveProgress()
        {
            SaveManager.SaveGame();
        }
        public bool IsUnlocked(string id)
        {
            return progress.TryGetValue(id, out var achievementProgress)
                   && achievementProgress.Unlocked;
        }

        public AchievementProgress GetProgress(string id)
        {
            progress.TryGetValue(id, out var achievementProgress);

            return achievementProgress;
        }

        public AchievementDefinition GetDefinition(string id)
        {
            definitions.TryGetValue(id, out var definition);

            return definition;
        }

        public IReadOnlyList<AchievementDefinition> GetAll()
        {
            return achievements;
        }
    }
}