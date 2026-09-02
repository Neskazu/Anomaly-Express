using Nac.Extensions;
using R3;
using UnityEngine;

namespace Localization
{
    [RequireComponent(typeof(Renderer))]
    public class LocalizedTexture : MonoBehaviour
    {
        [SerializeField] private string fileName;
        [SerializeField] private string textureProperty = "_BaseMap";

        private readonly CompositeDisposable disposables = new();

        private Renderer targetRenderer;
        private Material runtimeMaterial;

        public string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                Refresh();
            }
        }

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();

            runtimeMaterial = targetRenderer.material;
        }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        private void OnEnable()
        {
            LocalizationManager.Language
                .Subscribe(Refresh)
                .AddTo(disposables);
        }

        private void OnDisable()
        {
            disposables.Clear();
        }

        public void Refresh()
        {
            if (LocalizationManager.Instance == null || runtimeMaterial == null)
            {
                return;
            }

            var texture = LocalizationManager.Instance.GetTexture(fileName);
            if (texture != null)
            {
                runtimeMaterial.mainTexture = texture;
            }
        }

#if UNITY_EDITOR
        public void SetFileName(string value)
        {
            fileName = value;
        }
#endif
    }
}