using Cysharp.Threading.Tasks;
using SaveSystem;
using Scene;
using System.Collections.Generic;
using Train;
using Unity.Netcode;
using UnityEngine;

public class FinalLevelRoomController : NetworkBehaviour
{
    private const string GameCompletedKey = "GameCompleted";

    [SerializeField] private DoorController door;
    [SerializeField] private SceneTransitionSequence endGameSequence;

    private readonly HashSet<ulong> _clientsInside = new();

    private bool _isTransitioning;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        door.SetLockServerRpc(false);
        door.OnDoorStateChanged += HandleDoorState;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;

        door.OnDoorStateChanged -= HandleDoorState;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        var netObj = other.GetComponent<NetworkObject>();

        if (netObj == null || !netObj.IsPlayerObject)
            return;

        _clientsInside.Add(netObj.OwnerClientId);

        if (!_isTransitioning && IsAllPlayersInside())
            door.SetLockServerRpc(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer || _isTransitioning)
            return;

        var netObj = other.GetComponent<NetworkObject>();

        if (netObj == null || !netObj.IsPlayerObject)
            return;

        _clientsInside.Remove(netObj.OwnerClientId);
    }

    private void HandleDoorState(DoorController _, bool isOpen)
    {
        if (!IsServer)
            return;

        if (isOpen)
        {
            door.SetLockServerRpc(true);

            if (IsAllPlayersInside())
                door.SetLockServerRpc(false);
        }
        else
        {
            if (!_isTransitioning && IsAllPlayersInside())
            {
                door.SetLockServerRpc(true);
                EndGame().Forget();
            }
        }
    }

    private bool IsAllPlayersInside()
    {
        return _clientsInside.Count > 0 &&
               _clientsInside.Count == NetworkManager.Singleton.ConnectedClients.Count;
    }

    private async UniTaskVoid EndGame()
    {
        _isTransitioning = true;

        SaveManager.Save.Session.ShowFeedbackPopup = true;
        SaveManager.SaveGame();

        await SceneTransitionManager.Instance.Play(
            endGameSequence,
            showLoadingScreen: true);
    }
}