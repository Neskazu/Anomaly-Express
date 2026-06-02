using UnityEngine;

public class SnowClickEmitter : MonoBehaviour
{
    [SerializeField] private ParticleSystem clickSnow;
    [SerializeField] private Camera mainCamera;

    [SerializeField] private float spawnDistance = 10f;
    [SerializeField] private float distanceVariation = 3f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            pos.z = spawnDistance + Random.Range(0, distanceVariation);

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(pos);

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = worldPos;

            clickSnow.Emit(emitParams, 100);
        }
    }
}