using UnityEngine;

public class SnowClickEmitter : MonoBehaviour
{
    [SerializeField] private ParticleSystem clickSnow;
    [SerializeField] private Camera mainCamera;

    [SerializeField] private float spawnDistance = 10f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Input.mousePosition;
            pos.z = spawnDistance;

            Vector3 worldPos = mainCamera.ScreenToWorldPoint(pos);

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = worldPos;

            clickSnow.Emit(emitParams, 100);
        }
    }
}