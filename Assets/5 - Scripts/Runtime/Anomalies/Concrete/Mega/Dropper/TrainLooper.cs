using System.Collections.Generic;
using UnityEngine;

public class TrainLooper : MonoBehaviour
{
    [System.Serializable]
    public class Segment
    {
        public Transform transform;
        public float length = 22f;
    }

    [SerializeField] private List<Segment> segments;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float border = 10f;

    private void Update()
    {
        foreach (var segment in segments)
        {
            segment.transform.position += Vector3.forward * speed * Time.deltaTime;
        }

        Segment first = segments[0];

        if (first.transform.position.z >= border)
        {
            Segment last = segments[segments.Count - 1];

            float newZ = last.transform.position.z - first.length;

            first.transform.position = new Vector3(
                first.transform.position.x,
                first.transform.position.y,
                newZ);

            segments.RemoveAt(0);
            segments.Add(first);
        }
    }
}