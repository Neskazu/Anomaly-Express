using Newtonsoft.Json;
using SaveSystem;
using System.Collections.Generic;
using System.IO;
using Nac.Singleton;
using R3;
using TMPro;
using UnityEngine;

namespace Localization
{
    public class LocalizationManager : Service<LocalizationManager>
    {
        private static readonly ReactiveProperty<Language> CurrentLanguage = new();
        public static ReadOnlyReactiveProperty<Language> Language => CurrentLanguage;

        [SerializeField] private LocalizationFontDatabase fontDatabase;

        private readonly Dictionary<string, Texture2D> textureCache = new();
        private readonly Dictionary<string, string> localization = new();
        private readonly List<Language> languages = new();

        public IReadOnlyList<Language> AvailableLanguages => languages;

        private string LanguagesFolder
        {
            get
            {
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "2 - Data", "Languages");
#else
return Path.Combine(Application.dataPath, "..", "Data", "Languages");
#endif
            }
        }

        public TMP_FontAsset CurrentFont { get; private set; }

        public override void Awake()
        {
            base.Awake();

            LoadLanguages();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void LoadLanguages()
        {
            languages.Clear();
            textureCache.Clear();

            if (!Directory.Exists(LanguagesFolder))
            {
                return;
            }

            foreach (var folder in Directory.GetDirectories(LanguagesFolder))
            {
                var languageInfoPath = Path.Combine(folder, "language.json");

                if (!File.Exists(languageInfoPath))
                    continue;

                var info =
                    JsonConvert.DeserializeObject<LanguageInfo>(
                        File.ReadAllText(languageInfoPath));

                Language language = new()
                {
                    Info = info,
                    Folder = folder,
                    Flag = LoadFlag(Path.Combine(folder, info.Flag))
                };

                languages.Add(language);
            }
        }

        public void LoadLanguage(string code)
        {
            var language =
                languages.Find(x => x.Info.Code == code);

            if (language == null)
            {
                Debug.LogWarning($"Language '{code}' not found.");

                if (languages.Count == 0)
                    return;

                language = languages[0];
            }

            localization.Clear();
            textureCache.Clear();

            foreach (var file in Directory.GetFiles(language.Folder, "*.json"))
            {
                if (Path.GetFileName(file) == "language.json")
                    continue;

                var entries =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(file));

                foreach (var pair in entries)
                {
                    localization[pair.Key] = pair.Value;
                }
            }

            CurrentFont = fontDatabase.GetFont(language.Info.Code);
            CurrentLanguage.Value = language;

            SaveManager.Save.Settings.Language = language.Info.Code;
            SaveManager.SaveGame();

            Debug.Log($"Loaded language {language.Info.NativeName}");
        }

        public string Get(string key)
        {
            if (localization.TryGetValue(key, out var value))
            {
                return value;
            }

            return $"<{key}>";
        }

        private Sprite LoadFlag(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            Texture2D texture = new(2, 2);

            texture.LoadImage(File.ReadAllBytes(path));

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one * 0.5f);
        }

        public void SetLanguage(string code)
        {
            SaveManager.Save.Settings.Language = code;
            SaveManager.SaveGame();

            LoadLanguage(code);
        }

        public IReadOnlyList<Language> GetLanguages()
        {
            return languages;
        }

        public Language GetCurrentLanguage()
        {
            return languages.Find(x =>
                x.Info.Code == SaveManager.Save.Settings.Language);
        }

        public TMP_FontAsset GetFontForLanguage(string code)
        {
            return fontDatabase.GetFont(code);
        }

        public void Initialize()
        {
            LoadLanguage(SaveManager.Save.Settings.Language);
        }

        public Texture2D GetTexture(string fileName)
        {
            if (CurrentLanguage.CurrentValue == null)
            {
                return null;
            }

            var cacheKey = $"{CurrentLanguage.CurrentValue.Info.Code}/{fileName}";
            if (textureCache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var path = Path.Combine(CurrentLanguage.CurrentValue.Folder, fileName);
            if (!File.Exists(path))
            {
                var english = languages.Find(x => x.Info.Code == "en");

                if (english == null)
                    return null;

                path = Path.Combine(english.Folder, fileName);

                if (!File.Exists(path))
                    return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(File.ReadAllBytes(path)))
            {
                return null;
            }

            textureCache[cacheKey] = texture;
            return texture;
        }
    }
}