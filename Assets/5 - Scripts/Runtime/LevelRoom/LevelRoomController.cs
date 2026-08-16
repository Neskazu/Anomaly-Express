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
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        if (door != null) door.OnDoorStateChanged -= HandleDoorState;

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
            }
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (_clientsInside.Remove(clientId))
        {
            CheckDoorLockState();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsPlayerObject)
        {
            _clientsInside.Add(netObj.OwnerClientId);
            CheckDoorLockState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer || _lockTriggerExits) return;

        var netObj = other.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsPlayerObject)
        {
            _clientsInside.Remove(netObj.OwnerClientId);
            CheckDoorLockState();

            if (_isNewLevelLoaded && _clientsInside.Count == 0)
            {
                NetworkObject.Despawn();
            }
        }
    }
    private void CheckDoorLockState()
    {
        if (_isTransitioning || _isNewLevelLoaded) return;

        if (IsAllPlayersInside())
        {
            door.SetLockServerRpc(false);
        }
    }

    private void HandleDoorState(DoorController doorController, bool isOpen)
    {
        if (!IsServer) return;

        if (isOpen)
        {
            door.SetLockServerRpc(true);
            CheckDoorLockState();
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

        await SceneTransitionManager.Instance.Play(nextLevelSequence, showLoadingScreen: false);
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
            if (_playerLocalPositions.TryGetValue(clientId, out var localPos))
            {
                Vector3 newWorldPos = transform.TransformPoint(localPos);
                Quaternion newWorldRot = transform.rotation * _playerLocalRotations[clientId];

                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
                };

                TeleportPlayerClientRpc(newWorldPos, newWorldRot, clientRpcParams);
            }
        }

        door.SetLockServerRpc(false);
        _isNewLevelLoaded = true;
        _isTransitioning = false;

        CheckPositionsAfterFrames().Forget();
    }
    [ClientRpc]
    private void TeleportPlayerClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayer != null)
        {
            var characterController = localPlayer.GetComponent<CharacterController>();
            if (characterController != null) characterController.enabled = false;

            var playerNetTransform = localPlayer.GetComponent<NetworkTransform>();
            if (playerNetTransform != null)
            {
                playerNetTransform.Teleport(position, rotation, localPlayer.transform.localScale);
            }
            else
            {
                localPlayer.transform.position = position;
                localPlayer.transform.rotation = rotation;
            }

            if (characterController != null) characterController.enabled = true;
        }
    }

    private async UniTaskVoid CheckPositionsAfterFrames()
    {
        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitForFixedUpdate();

        _lockTriggerExits = false;
    }
}