using System;
using Anomalies;
using UnityEngine;

public class AnomalyMoose : AnomalyBase
{
    [SerializeField] private Transform moose;
    [SerializeField] private float speed = 2f;

    protected override void OnActivate()
    {
    }

    protected override void OnDeactivate()
    {
    }

    protected override void OnUpdate()
    {
        if (moose == null) return;

        moose.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }
}