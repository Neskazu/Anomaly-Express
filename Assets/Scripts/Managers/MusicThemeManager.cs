using Music;
using Scene;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class MusicThemeManager : MonoBehaviour
    {
        [System.Serializable]
        public struct SceneMusic
        {
            public SceneLoader.Scene sceneName;
            public MusicSettings musicSettings;
        }

        public SceneMusic[] sceneMusics;
        private static MusicThemeManager _instance;
        [SerializeField] private AudioSource musicThemeSource;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            foreach (var sceneMusic in sceneMusics)
            {
                if (sceneMusic.sceneName.ToString() == scene.name)
                {
                    PlayMusic(sceneMusic.musicSettings);
                    return;
                }
            }
        }

        private void PlayMusic(MusicSettings musicSettings)
        {
            if (!musicThemeSource && musicThemeSource.clip == musicSettings.clip)
            {
                return;
            }

            musicThemeSource.clip = musicSettings.clip;
            musicThemeSource.volume = musicSettings.volume;
            musicThemeSource.loop = true;
            musicThemeSource.Play();
        }
    }
}