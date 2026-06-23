using UnityEngine;

namespace Anomalies.Concrete.Visual
{
    [DefaultExecutionOrder(200)]
    public class AnomalyPassengerLookAt : AnomalyBase
    {
        [Header("Heads")]
        [SerializeField] private Transform[] heads;

        [Header("Look Settings")]
        [SerializeField] private float lookSpeed = 6f;


        [SerializeField] private Camera _camera;

        private Quaternion[] _currentRotations;

        protected override void OnActivate()
        {
            _camera = Camera.main;

            if (heads != null)
            {
                _currentRotations = new Quaternion[heads.Length];
                for (int i = 0; i < heads.Length; i++)
                {
                    if (heads[i] != null)
                        _currentRotations[i] = heads[i].rotation; 
                }
            }
        }

        protected override void OnDeactivate()
        {
        }

        private void LateUpdate()
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera == null || heads == null || heads.Length == 0 || _currentRotations == null)
                return;

            Vector3 targetPos = _camera.transform.position;

            for (int i = 0; i < heads.Length; i++)
            {
                Transform head = heads[i];
                if (head == null || head.parent == null)
                    continue;
                Vector3 dirWorld = targetPos - head.position;

                Quaternion targetWorldRot = Quaternion.LookRotation(dirWorld.normalized, Vector3.up);
                Quaternion targetLocalRot = Quaternion.Inverse(head.parent.rotation) * targetWorldRot;
                Quaternion finalTargetWorldRot = head.parent.rotation * targetLocalRot;
                _currentRotations[i] = Quaternion.Slerp(
                    _currentRotations[i],
                    finalTargetWorldRot,
                    Time.deltaTime * lookSpeed
                );
                head.rotation = _currentRotations[i];
            }
        }
    }
}