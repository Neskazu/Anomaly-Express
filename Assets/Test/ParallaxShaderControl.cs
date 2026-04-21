using UnityEngine;

namespace Parallax
{
    public class ParallaxShaderControl : MonoBehaviour
    {
        [Header("Global Settings")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private float chunkSizeZ = 200f; 

        private float _currentOffset;
        private readonly int ScrollOffsetID = Shader.PropertyToID("_SScrollOffset");

        void Update()
        {
            _currentOffset += Time.deltaTime * speed;
            float loopedOffset = _currentOffset % chunkSizeZ;
            Shader.SetGlobalFloat(ScrollOffsetID, -loopedOffset);
        }
    }
}