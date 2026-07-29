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

    [SerializeField] private Vector3 moveDirection = Vector3.forward;

    private void Update()
    {
        foreach (var segment in segments)
        {
            segment.transform.position += moveDirection * speed * Time.deltaTime;
        }

        Segment first = segments[0];

        if (Vector3.Dot(first.transform.position, moveDirection) >= border)
        {
            Segment last = segments[segments.Count - 1];
            first.transform.position = last.transform.position - (moveDirection * first.length);

            segments.RemoveAt(0);
            segments.Add(first);
        }
    }
}