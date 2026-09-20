using System.IO;
using R3;
using UnityEngine;

namespace SaveSystem
{
    public static class SaveManager
    {
        private const string SaveFileName = "save.json";

        public static GameSave Save { get; private set; }

        private static string SavePath =>
            Path.Combine(Application.persistentDataPath, SaveFileName);

        private static readonly Subject<Unit> saved = new();
        private static readonly Subject<Unit> loaded = new();

        public static Observable<Unit> OnSaved => saved;
        public static Observable<Unit> OnLoaded => loaded;

        public static void Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("Save file not found. Creating new save.");

                Save = CreateNewSave();
                SaveGame();

                return;
            }

            try
            {
                var json = File.ReadAllText(SavePath);

                Save = JsonUtility.FromJson<GameSave>(json);

                if (Save == null)
                {
                    Debug.LogWarning("Save file was invalid. Creating new save.");

                    Save = CreateNewSave();
                }

                loaded.OnNext(Unit.Default);
            }
            catch
            {
                Debug.LogError("Failed to load save.");

                Save = CreateNewSave();
            }
        }

        public static void SaveGame()
        {
            try
            {
                var json = JsonUtility.ToJson(Save, true);

                File.WriteAllText(SavePath, json);

                saved.OnNext(Unit.Default);
                Debug.Log("Game saved.");
            }
            catch
            {
                Debug.LogError("Failed to save game.");
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            Save = CreateNewSave();
        }

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }

        private static GameSave CreateNewSave()
        {
            return new GameSave();
        }
    }
}