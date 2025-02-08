using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicThemeManager : MonoBehaviour
{
    [System.Serializable]
    public struct SceneMusic
    {
        public SceneLoader.Scene sceneName;
        public MusicSettings musicSettings;
    }
    public SceneMusic[] sceneMusics;
    private static MusicThemeManager instance;
    [SerializeField]
    private AudioSource musicThemeSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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
        if (musicThemeSource.clip == musicSettings.clip)
        {
            return;
        }
        musicThemeSource.clip = musicSettings.clip;
        musicThemeSource.volume = musicSettings.volume;
        musicThemeSource.loop = true;
        musicThemeSource.Play();
    }
}

