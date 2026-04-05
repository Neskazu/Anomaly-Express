using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Concrete
{
    public class UiTransitionController : MonoBehaviour
    {
        [SerializeField] private Transition[] transitions;

        private void Awake()
        {
            foreach (var transition in transitions)
            {
                transition.button.onClick.AddListener(() =>
                {
                    transition.from.Hide();
                    transition.to.Show();
                });
            }
        }

        [Serializable]
        public struct Transition
        {
            public Button button;
            public UiTransition from;
            public UiTransition to;
        }
    }
}