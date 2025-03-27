using System;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scene
{
    [CreateAssetMenu(fileName = "Scene Transition Sequence", menuName = "Configs/Scene Transition Sequence", order = 0)]
    public class SceneTransitionSequence : ScriptableObject
    {
        [Header("BGM Settings")]
        public AudioClip bgm;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Header("Loading steps")]
        public SceneTransitionStep[] steps;

        [Serializable]
        public struct SceneTransitionStep
        {
            public SceneReference scene;
            public LoadSceneMode loadMode;
            public NetworkMode networkMode;
        }

        [Serializable]
        public enum NetworkMode
        {
            Solo,
            Network
        }
    }
}