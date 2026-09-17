using Nac.Singleton;
using Scene;
using R3;
using UnityEngine;

namespace Managers
{
    public class MusicThemeManager : Service<MusicThemeManager>
    {
        [SerializeField] private AudioSource musicThemeSource;

        public override void Awake()
        {
            base.Awake();

            SceneTransitionManager.Instance.PostLoading
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