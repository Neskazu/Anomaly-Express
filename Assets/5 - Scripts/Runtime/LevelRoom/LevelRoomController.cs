using Anomalies;
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

        if (NetworkObject != null)
        {
            NetworkObject.DestroyWithScene = false;
        }

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
        if (_clientsInside.Count == 0) return false;

        if (SplitControlAnomaly.IsSplitActive)
        {
            return true;
        }

        return _clientsInside.Count == NetworkManager.Singleton.ConnectedClients.Count;
    }

    /*
     * Caches local positions of players relative to the room and starts the scene transition.
     * Triggers are locked to prevent false exit events during the teleportation process.
     */
    private async UniTaskVoid StartLevelTransition()
    {
        _isTransitioning = true;
        _lockTriggerExits = true;

        _playerLocalPositions.Clear();
        _playerLocalRotations.Clear();

        IEnumerable<ulong> clientsToTeleport = SplitControlAnomaly.IsSplitActive
            ? NetworkManager.Singleton.ConnectedClients.Keys
            : _clientsInside;

        NetworkObject hostPlayerObject = null;
        if (SplitControlAnomaly.IsSplitActive && NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.ServerClientId, out var hostClient))
        {
            hostPlayerObject = hostClient.PlayerObject;
        }

        foreach (ulong clientId in clientsToTeleport)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client) && client.PlayerObject != null)
            {
                Transform targetTransform = (SplitControlAnomaly.IsSplitActive && hostPlayerObject != null)
                    ? hostPlayerObject.transform
                    : client.PlayerObject.transform;

                _playerLocalPositions[clientId] = transform.InverseTransformPoint(targetTransform.position);
                _playerLocalRotations[clientId] = Quaternion.Inverse(transform.rotation) * targetTransform.rotation;
            }
        }

        await SceneTransitionManager.Instance.Play(nextLevelSequence, showLoadingScreen: false);
    }

    /*
     * Called on the server when the scene finishes loading.
     * Moves the room to origin, forces clients to sync the room position locally, 
     * and teleports players back inside.
     */
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

        SyncRoomPositionClientRpc(Vector3.zero, Quaternion.identity);

        foreach (var kvp in _playerLocalPositions)
        {
            ulong clientId = kvp.Key;
            Vector3 localPos = kvp.Value;

            Vector3 newWorldPos = transform.TransformPoint(localPos);
            Quaternion newWorldRot = transform.rotation * _playerLocalRotations[clientId];

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            };

            TeleportPlayerClientRpc(newWorldPos, newWorldRot, clientRpcParams);
        }

        door.SetLockServerRpc(false);
        _isNewLevelLoaded = true;
        _isTransitioning = false;

        UnlockTriggersAfterDelayAsync().Forget();
    }

    [ClientRpc]
    private void SyncRoomPositionClientRpc(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        WaitAndTeleportPlayerAsync(position, rotation).Forget();
    }

    /*
     * Waits for the local player object to be spawned by NGO on the client side after a scene load.
     * Once found, teleports the player to the exact coordinates inside the room.
     */
    private async UniTaskVoid WaitAndTeleportPlayerAsync(Vector3 position, Quaternion rotation)
    {
        float timeout = 5f;

        while (NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject() == null && timeout > 0)
        {
            await UniTask.Yield();
            timeout -= Time.deltaTime;
        }

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

    /*
     * Delays unlocking the triggers to ensure all clients have successfully spawned and teleported.
     * Prevents false server-side OnTriggerExit events caused by network latency or scene loading times.
     */
    private async UniTaskVoid UnlockTriggersAfterDelayAsync()
    {
        await UniTask.Delay(2000);
        _lockTriggerExits = false;
    }
}