using UnityEngine;

namespace Anomalies.Blood
{
    [RequireComponent(typeof(Renderer))]
    public class BloodSpreadTester : MonoBehaviour
    {
        [Header("Testing Parameters")]
        [Tooltip("How long it takes to spread fully (in seconds)")]
        public float spreadDuration = 10f;
        
        [Tooltip("Automatically loop the spread animation")]
        public bool loop = true;

        private Material _material;
        private float _timer = 0f;

        private void Start()
        {
            // Create an instance so we don't modify the shared asset
            _material = GetComponent<Renderer>().material;
        }

        private void Update()
        {
            if (_material == null) return;

            _timer += Time.deltaTime;
            
            // Calculate flow from 0 to 1
            float flow = Mathf.Clamp01(_timer / spreadDuration);
            
            // Send parameter to shader
            _material.SetFloat("_Flow", flow);

            // Wait 2s after full spread, then loop
            if (loop && _timer > spreadDuration + 2f) 
            {
                _timer = 0f;
            }
        }
    }
}
