using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Parallax
{
    public class Parallax : MonoBehaviour
    {
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private Vector3 chunkSize;

        [SerializeField] private float speed;
        [SerializeField] private float size;

        private GameObject[] chunks;

        Vector3 spawnPosition;
        Vector3 despawnPosition;

        private void Awake()
        {
            Spawn();
        }

        private void Spawn()
        {
            var direction = transform.forward;

            spawnPosition = transform.position - direction * (size / 2);
            despawnPosition = transform.position + direction * (size / 2);

            var spacing = chunkSize.z;
            var amount = Mathf.CeilToInt(size / spacing);

            chunks = new GameObject[amount];

            for (int i = 0; i < amount; i++)
            {
                var position = spawnPosition + direction * spacing * i;
                var rotation = Quaternion.LookRotation(direction, Vector3.up);

                chunks[i] = Instantiate(chunkPrefab, position, rotation, transform);
            }
        }

        private void FixedUpdate()
        {
            foreach (var chunk in chunks)
            {
                if (!chunk)
                    continue;

                chunk.transform.position += transform.forward * (speed * Time.deltaTime);

                if (chunk.transform.position.z > despawnPosition.z)
                    chunk.transform.position = spawnPosition;
            }
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

            Vector3[] centers = new Vector3[amount];

            for (int i = 0; i < amount; i++)
            {
                Gizmos.matrix = Matrix4x4.TRS(spawn + direction * spacing * i,
                    Quaternion.LookRotation(direction, Vector3.up), Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, chunkSize);
            }
        }
    }
}