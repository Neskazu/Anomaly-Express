using Anomalies;
using DG.Tweening;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using KinematicCharacterController;

public class AnomalyPlayerSize : AnomalyBase
{
    [Header("Visual Settings")]
    [SerializeField] private float targetVisualMultiplier = 0.5f;
    [SerializeField] private string visualsNodeName = "Root";

    [Header("Physics Settings")]
    [SerializeField] private float targetYOffset = 0.75f;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float durationBack = 1.5f;

    private readonly Dictionary<ulong, PlayerInitialState> _initialStates = new Dictionary<ulong, PlayerInitialState>();

    private struct PlayerInitialState
    {
        public Transform VisualTransform;
        public Vector3 OriginalVisualScale;
        public float Height;
        public float Radius;
        public float YOffset;
    }

    protected override void OnActivate()
    {
        _initialStates.Clear();

        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.IsPlayerObject)
            {
                ulong clientId = netObj.OwnerClientId;
                var motor = netObj.GetComponent<KinematicCharacterMotor>();
                Transform visualRoot = netObj.transform.Find(visualsNodeName);

                if (motor != null && visualRoot != null)
                {
                    _initialStates[clientId] = new PlayerInitialState
                    {
                        VisualTransform = visualRoot,
                        OriginalVisualScale = visualRoot.localScale,
                        Height = motor.Capsule.height,
                        Radius = motor.Capsule.radius,
                        YOffset = motor.Capsule.center.y // По дефолту здесь 0
                    };

                    var state = _initialStates[clientId];

                    visualRoot.DOScale(state.OriginalVisualScale * targetVisualMultiplier, duration)
                        .SetId($"VisualScale_{clientId}");

                    // Твиним от текущего (0) до targetYOffset (0.75f)
                    float currentOffset = state.YOffset;
                    DOTween.To(() => currentOffset, x =>
                    {
                        currentOffset = x;
                        motor.SetCapsuleDimensions(
                            state.Radius,
                            state.Height,
                            currentOffset
                        );
                    }, targetYOffset, duration)
                    .SetId($"PhysScale_{clientId}");
                }
            }
        }
    }

    protected override void OnDeactivate()
    {
        Debug.Log("deactive");
        foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (netObj.IsPlayerObject && _initialStates.TryGetValue(netObj.OwnerClientId, out var state))
            {
                ulong clientId = netObj.OwnerClientId;
                var motor = netObj.GetComponent<KinematicCharacterMotor>();

                DOTween.Kill($"VisualScale_{clientId}");
                DOTween.Kill($"PhysScale_{clientId}");

                if (state.VisualTransform != null)
                {
                    state.VisualTransform.DOScale(state.OriginalVisualScale, durationBack);
                }

                if (motor != null)
                {
                    // Берем текущий Y Offset (на случай, если анимация прервалась на середине)
                    // и плавно возвращаем к изначальному (state.YOffset, то есть к 0)
                    float currentOffset = motor.Capsule.center.y;

                    DOTween.To(() => currentOffset, x =>
                    {
                        currentOffset = x;
                        motor.SetCapsuleDimensions(
                            state.Radius,
                            state.Height,
                            currentOffset
                        );
                    }, state.YOffset, durationBack);
                }
            }
        }

        _initialStates.Clear();
    }

    protected override void OnUpdate() { }
}