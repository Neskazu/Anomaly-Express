using Scene;
using R3;
using UnityEngine;

namespace Managers
{
    public class MusicThemeManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicThemeSource;

        private static MusicThemeManager _instance;

        private void Awake()
        {
            if (_instance)
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            _instance = this;

            SceneTransitionManager.Instance.Loaded
                .Subscribe(PlayMusic)
                .AddTo(this);
        }

        private void PlayMusic(SceneTransitionSequence sequence)
        {
            if (!sequence.bgm)
            {
                musicThemeSource.Stop();
                return;
            }

            if (musicThemeSource.clip == sequence.bgm)
                return;

            musicThemeSource.clip = sequence.bgm;
            musicThemeSource.volume = sequence.volume;
            musicThemeSource.loop = true;
            musicThemeSource.Play();
        }
    }
}