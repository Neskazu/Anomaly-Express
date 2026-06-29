using Cysharp.Threading.Tasks;
using Scene;
using System.Collections.Generic;
using Train;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class LevelRoomController : NetworkBehaviour
{
    [SerializeField] private DoorController door;
    [SerializeField] private SceneTransitionSequence nextLevelSequence;

    private HashSet<ulong> _clientsInside = new HashSet<ulong>();
    private Dictionary<ulong, Vector3> _playerLocalPositions = new Dictionary<ulong, Vector3>();
    private Dictionary<ulong, Quaternion> _playerLocalRotations = new Dictionary<ulong, Quaternion>();

    private bool _isTransitioning = false;
    private bool _isNewLevelLoaded = false;
    private bool _lockTriggerExits = false;

    public override void OnNetworkSpawn()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (!IsServer) return;

        door.SetLockServerRpc(false);
        door.OnDoorStateChanged += HandleDoorState;

        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            door.OnDoorStateChanged -= HandleDoorState;
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsPlayerObject)
        {
            _clientsInside.Add(netObj.OwnerClientId);

            if (!_isTransitioning && IsAllPlayersInside())
            {
                door.SetLockServerRpc(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer || _lockTriggerExits) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsPlayerObject)
        {
            _clientsInside.Remove(netObj.OwnerClientId);

            if (_isNewLevelLoaded && _clientsInside.Count == 0)
            {
                NetworkObject.Despawn();
            }
        }
    }

    private void HandleDoorState(DoorController doorController, bool isOpen)
    {
        if (!IsServer) return;

        if (isOpen)
        {
            door.SetLockServerRpc(true);
            if (IsAllPlayersInside()) door.SetLockServerRpc(false);
        }
        else
        {
            if (!_isTransitioning && !_isNewLevelLoaded && IsAllPlayersInside())
            {
                door.SetLockServerRpc(true);
                StartLevelTransition().Forget();
            }
        }
    }

    private bool IsAllPlayersInside()
    {
        return _clientsInside.Count > 0 && _clientsInside.Count == NetworkManager.Singleton.ConnectedClients.Count;
    }

    private async UniTaskVoid StartLevelTransition()
    {
        _isTransitioning = true;
        _lockTriggerExits = true;

        _playerLocalPositions.Clear();
        _playerLocalRotations.Clear();

        foreach (ulong clientId in _clientsInside)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
            {
                _playerLocalPositions[clientId] = transform.InverseTransformPoint(client.PlayerObject.transform.position);
                _playerLocalRotations[clientId] = Quaternion.Inverse(transform.rotation) * client.PlayerObject.transform.rotation;
            }
        }

        await SceneTransitionController.Instance.Play(nextLevelSequence, showLoadingScreen: false);
    }

    private void OnSceneLoaded(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!IsServer || !_isTransitioning) return;

        var netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport(Vector3.zero, Quaternion.identity, transform.localScale);
        }
        else
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }

        foreach (ulong clientId in _clientsInside)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
            {
                if (_playerLocalPositions.TryGetValue(clientId, out var localPos))
                {
                    Vector3 newWorldPos = transform.TransformPoint(localPos);
                    Quaternion newWorldRot = transform.rotation * _playerLocalRotations[clientId];

                    var playerNetTransform = client.PlayerObject.GetComponent<NetworkTransform>();
                    if (playerNetTransform != null)
                    {
                        playerNetTransform.Teleport(newWorldPos, newWorldRot, client.PlayerObject.transform.localScale);
                    }
                    else
                    {
                        client.PlayerObject.transform.position = newWorldPos;
                        client.PlayerObject.transform.rotation = newWorldRot;
                    }
                }
            }
        }

        door.SetLockServerRpc(false);
        _isNewLevelLoaded = true;
        _isTransitioning = false;

        CheckPositionsAfterFrames().Forget();
    }

    private async UniTaskVoid CheckPositionsAfterFrames()
    {
        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();

        _lockTriggerExits = false;
    }
}