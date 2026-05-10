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
    [SerializeField] private float targetPhysicsMultiplier = 0.82f;

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
                        YOffset = motor.Capsule.center.y
                    };

                    var state = _initialStates[clientId];
                    visualRoot.DOScale(state.OriginalVisualScale * targetVisualMultiplier, duration)
                        .SetId($"VisualScale_{clientId}");
                    float lerpVal = 1f;
                    DOTween.To(() => lerpVal, x =>
                    {
                        lerpVal = x;
                        motor.SetCapsuleDimensions(
                            state.Radius * lerpVal,
                            state.Height * lerpVal,
                            state.YOffset * lerpVal
                        );
                    }, targetPhysicsMultiplier, duration)
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
                    float currentPhysMultiplier = motor.Capsule.height / state.Height;

                    DOTween.To(() => currentPhysMultiplier, x =>
                    {
                        currentPhysMultiplier = x;
                        motor.SetCapsuleDimensions(
                            state.Radius * currentPhysMultiplier,
                            state.Height * currentPhysMultiplier,
                            state.YOffset * currentPhysMultiplier
                        );
                    }, 1f, durationBack);
                }
            }
        }

        _initialStates.Clear();
    }

    protected override void OnUpdate() { }
}