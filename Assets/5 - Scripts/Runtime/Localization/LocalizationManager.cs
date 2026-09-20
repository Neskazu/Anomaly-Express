using Newtonsoft.Json;
using SaveSystem;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Nac.Singleton;
using R3;
using TMPro;
using UnityEngine;

namespace Localization
{
    [System.Serializable]
    public class LocalizationManifest
    {
        public List<string> Languages = new();
        public Dictionary<string, List<string>> FilesPerLanguage = new();
    }

    public class LocalizationManager : Service<LocalizationManager>
    {
        private static readonly ReactiveProperty<Language> CurrentLanguage = new();
        public static ReadOnlyReactiveProperty<Language> Language => CurrentLanguage;

        [SerializeField] private LocalizationFontDatabase fontDatabase;

        private readonly Dictionary<string, Texture2D> textureCache = new();
        private readonly Dictionary<string, string> fallbackLocalization = new();
        private readonly Dictionary<string, string> localization = new();
        private readonly List<Language> languages = new();

        public IReadOnlyList<Language> AvailableLanguages => languages;

        private string LanguagesFolder => Path.Combine(Application.streamingAssetsPath, "Languages");

        public TMP_FontAsset CurrentFont { get; private set; }

        public bool IsReady { get; private set; } = false;
        private bool isInitializing = false;

        private LocalizationManifest manifest;

        public async Task InitializeAsync()
        {
            if (IsReady || isInitializing) return;

            isInitializing = true;

            try
            {
#if UNITY_EDITOR
                GenerateManifestEditor();
#endif
                await LoadManifestAsync();
                await LoadLanguagesAsync();
                await LoadFallbackLanguageAsync();

                string savedLang = SaveManager.Save?.GeneralSettings?.Language;
                await LoadLanguageAsync(string.IsNullOrEmpty(savedLang) ? "en" : savedLang);

                IsReady = true;
            }
            finally
            {
                isInitializing = false;
            }
        }

        private async Task LoadManifestAsync()
        {
            string path = Path.Combine(LanguagesFolder, "manifest.json");
            string json = await ReadTextFileAsync(path);

            if (!string.IsNullOrEmpty(json))
                manifest = JsonConvert.DeserializeObject<LocalizationManifest>(json);
            else
                Debug.LogError("[Localization] manifest.json ÌÂ Ì‡È‰ÂÌ!");
        }

#if UNITY_EDITOR
        private void GenerateManifestEditor()
        {
            if (!Directory.Exists(LanguagesFolder)) return;

            var newManifest = new LocalizationManifest();
            foreach (var folder in Directory.GetDirectories(LanguagesFolder))
            {
                string langCode = new DirectoryInfo(folder).Name;
                newManifest.Languages.Add(langCode);
                newManifest.FilesPerLanguage[langCode] = new List<string>();

                foreach (var file in Directory.GetFiles(folder, "*.json"))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName != "language.json")
                        newManifest.FilesPerLanguage[langCode].Add(fileName);
                }
            }
            File.WriteAllText(Path.Combine(LanguagesFolder, "manifest.json"), JsonConvert.SerializeObject(newManifest, Formatting.Indented));
        }
#endif

        private async Task LoadLanguagesAsync()
        {
            languages.Clear();
            textureCache.Clear();

            if (manifest == null) return;

            foreach (var langFolder in manifest.Languages)
            {
                string folderPath = Path.Combine(LanguagesFolder, langFolder);
                string languageInfoPath = Path.Combine(folderPath, "language.json");

                string infoJson = await ReadTextFileAsync(languageInfoPath);
                if (string.IsNullOrEmpty(infoJson)) continue;

                var info = JsonConvert.DeserializeObject<LanguageInfo>(infoJson);

                Language language = new()
                {
                    Info = info,
                    Folder = folderPath,
                    Flag = await LoadFlagAsync(Path.Combine(folderPath, info.Flag))
                };

                languages.Add(language);
            }
        }

        private async Task LoadFallbackLanguageAsync()
        {
            fallbackLocalization.Clear();
            var englishLanguage = languages.Find(x => x.Info.Code == "en");

            if (englishLanguage == null || manifest == null) return;

            // »—œ–¿¬À≈Õ»≈: »ÒÔÓÎ¸ÁÛÂÏ Info.Code ‚ÏÂÒÚÓ DirectoryInfo
            string langCode = englishLanguage.Info.Code;
            if (manifest.FilesPerLanguage.TryGetValue(langCode, out var files))
            {
                var tasks = new List<Task<string>>();
                foreach (var fileName in files)
                {
                    string filePath = Path.Combine(englishLanguage.Folder, fileName);
                    tasks.Add(ReadTextFileAsync(filePath));
                }

                string[] jsonResults = await Task.WhenAll(tasks);

                foreach (var json in jsonResults)
                {
                    if (string.IsNullOrEmpty(json)) continue;
                    var entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    foreach (var pair in entries)
                        fallbackLocalization[pair.Key] = pair.Value;
                }
            }
        }

        public async Task LoadLanguageAsync(string code)
        {
            var language = languages.Find(x => x.Info.Code == code);

            if (language == null)
            {
                Debug.LogWarning($"Language '{code}' not found.");
                if (languages.Count == 0) return;
                language = languages[0];
            }

            localization.Clear();
            textureCache.Clear();

            // »—œ–¿¬À≈Õ»≈: »ÒÔÓÎ¸ÁÛÂÏ Info.Code ‚ÏÂÒÚÓ DirectoryInfo
            string langCode = language.Info.Code;
            if (manifest != null && manifest.FilesPerLanguage.TryGetValue(langCode, out var files))
            {
                var tasks = new List<Task<string>>();
                foreach (var fileName in files)
                {
                    string filePath = Path.Combine(language.Folder, fileName);
                    tasks.Add(ReadTextFileAsync(filePath));
                }

                string[] jsonResults = await Task.WhenAll(tasks);

                foreach (var json in jsonResults)
                {
                    if (string.IsNullOrEmpty(json)) continue;
                    var entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    foreach (var pair in entries)
                        localization[pair.Key] = pair.Value;
                }
            }

            CurrentFont = fontDatabase.GetFont(language.Info.Code);
            CurrentLanguage.Value = language;

            if (SaveManager.Save?.GeneralSettings != null)
            {
                SaveManager.Save.GeneralSettings.Language = language.Info.Code;
                SaveManager.SaveGame();
            }

            Debug.Log($"Loaded language {language.Info.NativeName}");
        }

        public string Get(string key)
        {
            if (localization.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value)) return value;
            if (fallbackLocalization.TryGetValue(key, out var fallbackValue) && !string.IsNullOrEmpty(fallbackValue)) return fallbackValue;

            return $"<{key}>";
        }

        public async Task<Texture2D> GetTextureAsync(string fileName)
        {
            if (CurrentLanguage.CurrentValue == null) return null;

            var cacheKey = $"{CurrentLanguage.CurrentValue.Info.Code}/{fileName}";
            if (textureCache.TryGetValue(cacheKey, out var cached)) return cached;

            var path = Path.Combine(CurrentLanguage.CurrentValue.Folder, fileName);
            Texture2D texture = await LoadTextureAsync(path);

            if (texture == null)
            {
                var english = languages.Find(x => x.Info.Code == "en");
                if (english != null)
                {
                    path = Path.Combine(english.Folder, fileName);
                    texture = await LoadTextureAsync(path);
                }
            }

            if (texture != null) textureCache[cacheKey] = texture;
            return texture;
        }

        private async Task<Sprite> LoadFlagAsync(string path)
        {
            Texture2D texture = await LoadTextureAsync(path);
            if (texture == null) return null;

            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one * 0.5f);
        }

        public void SetLanguage(string code)
        {
            SaveManager.Save.GeneralSettings.Language = code;
            SaveManager.SaveGame();
            _ = LoadLanguageAsync(code);
        }

        private Task<string> ReadTextFileAsync(string path)
        {
            path = path.Replace("\\", "/");

#if UNITY_WEBGL && !UNITY_EDITOR
            var tcs = new TaskCompletionSource<string>();
            UnityWebRequest www = UnityWebRequest.Get(path);
            var operation = www.SendWebRequest();
            
            operation.completed += _ =>
            {
                if (www.result == UnityWebRequest.Result.Success)
                    tcs.SetResult(www.downloadHandler.text);
                else
                    tcs.SetResult(null);
                    
                www.Dispose();
            };
            return tcs.Task;
#else
            if (File.Exists(path)) return File.ReadAllTextAsync(path);
            return Task.FromResult<string>(null);
#endif
        }

        private Task<Texture2D> LoadTextureAsync(string path)
        {
            path = path.Replace("\\", "/");

#if UNITY_WEBGL && !UNITY_EDITOR
            var tcs = new TaskCompletionSource<Texture2D>();
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
            var operation = www.SendWebRequest();
            
            operation.completed += _ =>
            {
                if (www.result == UnityWebRequest.Result.Success)
                    tcs.SetResult(DownloadHandlerTexture.GetContent(www));
                else
                    tcs.SetResult(null);
                    
                www.Dispose();
            };
            return tcs.Task;
#else
            if (File.Exists(path))
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(File.ReadAllBytes(path));
                return Task.FromResult(texture);
            }
            return Task.FromResult<Texture2D>(null);
#endif
        }

        public IReadOnlyList<Language> GetLanguages() => languages;
        public TMP_FontAsset GetFontForLanguage(string code) => fontDatabase.GetFont(code);

        public Language GetCurrentLanguage()
        {
            return languages.Find(x => x.Info.Code == SaveManager.Save.GeneralSettings.Language);
        }
    }
}