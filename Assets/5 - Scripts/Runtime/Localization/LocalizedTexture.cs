using System.IO;
using UnityEngine;

namespace Localization
{
    [RequireComponent(typeof(Renderer))]
    public class LocalizedTexture : MonoBehaviour
    {
        [SerializeField]
        private string fileName;

        [SerializeField]
        private string textureProperty = "_BaseMap";

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                Refresh();
            }
        }

        private Renderer targetRenderer;
        private Material runtimeMaterial;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();

            runtimeMaterial = targetRenderer.material;
        }

        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.Instance.OnLanguageChanged -= Refresh;
        }

        public void Refresh()
        {
            if (LocalizationManager.Instance == null || runtimeMaterial == null)
                return;

            Texture2D texture = LocalizationManager.Instance.GetTexture(fileName);

            if (texture != null)
                runtimeMaterial.mainTexture = texture;
        }

#if UNITY_EDITOR
        public void SetFileName(string value)
        {
            fileName = value;
        }
#endif
    }
}