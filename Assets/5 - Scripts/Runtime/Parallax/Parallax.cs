using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Parallax
{
    public class Parallax : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private Vector3 chunkSize;

        [Header("Settings")]
        [SerializeField] private float size;
        [SerializeField] private float time;

        private Transform[] _chunks;

        private void Awake()
        {
            Spawn();
            Move();
        }

        private void Spawn()
        {
            var direction = transform.forward;
            var spawnPosition = transform.position - direction * (size / 2);
            var spacing = chunkSize.z;
            var amount = Mathf.CeilToInt(size / spacing);

            _chunks = new Transform[amount];

            for (int i = 0; i < amount; i++)
            {
                var position = spawnPosition + direction * spacing * i;
                var rotation = Quaternion.LookRotation(direction, Vector3.up);

                _chunks[i] = Instantiate(chunkPrefab, position, rotation, transform).transform;
            }
        }

        private void Move()
        {
            Sequence sequence = DOTween.Sequence();

            foreach (Transform chunk in _chunks)
            {
                sequence.Join(
                    chunk
                        .DOLocalMove(chunk.localPosition + Vector3.forward * chunkSize.z, time)
                        .SetEase(Ease.Linear)
                );
            }

            sequence.SetLoops(-1);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 direction = transform.forward;

            Vector3 spawn = transform.position - direction * (size / 2);
            Vector3 despawn = transform.position + direction * (size / 2);

            Handles.DrawWireDisc(spawn, direction, 5);
            Handles.DrawWireDisc(despawn, -direction, 5);
            Handles.DrawDottedLine(spawn, despawn, 5);

            float spacing = chunkSize.z;
            int amount = Mathf.CeilToInt(size / spacing);

            for (int i = 0; i < amount; i++)
            {
                Gizmos.matrix = Matrix4x4.TRS(spawn + direction * spacing * i, Quaternion.LookRotation(direction, Vector3.up), Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, chunkSize);
            }
        }
    }
}