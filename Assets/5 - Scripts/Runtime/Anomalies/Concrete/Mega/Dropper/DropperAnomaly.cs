using Anomalies;
using KinematicCharacterController;
using Player.Components;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DropperAnomaly : AnomalyBase, IKccHitReceiver
{

    [SerializeField] private Transform _startPoint;

    [Header("Group Settings")]
    [SerializeField] private GameObject[] _obstacles;

    [Header("Floating Settings")]
    [SerializeField] private float amplitude = 0.2f;
    [SerializeField] private float frequency = 1f;

    private Vector3[] _startPositions;
    private float[] _phaseOffsets;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        int count = _obstacles.Length;
        _startPositions = new Vector3[count];
        _phaseOffsets = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (_obstacles[i] != null)
            {
                _startPositions[i] = _obstacles[i].transform.position;
                _phaseOffsets[i] = Random.Range(0f, 100f);
                var router = _obstacles[i].AddComponent<DropperHitRouter>();
                router.Anomaly = this;
            }
        }
        OnAnomalyStateChanged += OnAnomalyToggled;
    }

    private void Start()
    {
        if (IsServer) Activate();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OnAnomalyStateChanged -= OnAnomalyToggled;
    }
    private void OnAnomalyToggled()
    {
        StartCoroutine(WaitAndChangeJumpState());
    }

    private IEnumerator WaitAndChangeJumpState()
    {
        while (NetworkManager.Singleton.LocalClient?.PlayerObject == null)
            yield return null;

        var player = NetworkManager.Singleton.LocalClient.PlayerObject;
        var jumpComp = player.GetComponent<PlayerJump>();

        if (jumpComp != null)
        {
            jumpComp.IsJumpEnabled = IsActive;
        }
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < _obstacles.Length; i++)
        {
            if (_obstacles[i] == null) continue;
            float offsetY = Mathf.Sin(Time.time * frequency + _phaseOffsets[i]) * amplitude;
            _obstacles[i].transform.position = _startPositions[i] + new Vector3(0, offsetY, 0);
        }
    }
    public void HandlePlayerFall(KinematicCharacterMotor motor)
    {
        Transform targetPoint = _startPoint;

        if (targetPoint != null)
        {
            motor.SetPositionAndRotation(targetPoint.position, targetPoint.rotation);
            motor.BaseVelocity = Vector3.zero;
        }
    }
    public void OnKccHit(KinematicCharacterMotor motor)
    {
        if (!IsActive) return;

        var netObj = motor.GetComponent<NetworkObject>();

        if (netObj != null && netObj.IsOwner)
        {
            StartCoroutine(SafeTeleportCoroutine(motor));
        }
    }

    private IEnumerator SafeTeleportCoroutine(KinematicCharacterMotor motor)
    {
        yield return null;
        if (motor != null)
        {
            HandlePlayerFall(motor);
        }
    }
    protected override void OnActivate()
    {
      
    }

    protected override void OnDeactivate()
    {
       
    }
}
public class DropperHitRouter : MonoBehaviour, IKccHitReceiver
{
    public DropperAnomaly Anomaly { get; set; }

    public void OnKccHit(KinematicCharacterMotor motor)
    {
        if (Anomaly != null)
        {
            Anomaly.OnKccHit(motor);
        }
    }
}

