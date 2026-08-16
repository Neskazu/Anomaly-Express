using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nac.Singleton;
using R3;
using Unity.Netcode;
using UnityEngine;

namespace Nac.Network
{
    public class NetworkController : Service<NetworkController>
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        [SerializeField] private NetworkConfig config;

        private readonly ReactiveProperty<bool> online = new();
        private readonly Subject<ulong> onClientConnected = new();
        private readonly Subject<ulong> onClientDisconnected = new();

        private UniTaskCompletionSource<bool> networkSource;

        public ReadOnlyReactiveProperty<bool> Online => online;
        public Observable<ulong> OnClientConnected => onClientConnected;
        public Observable<ulong> OnClientDisconnected => onClientDisconnected;

        #region Unity

        public override void Awake()
        {
            base.Awake();

            NetworkManager.OnInstantiated += OnManagerInstantiatedCallback;
            NetworkManager.OnDestroying += OnManagerDestroyCallback;

            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += onClientConnected.OnNext;
                NetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnected.OnNext;
            }

            onClientConnected.Subscribe(OnClientConnectedCallback).AddTo(this);
            onClientDisconnected.Subscribe(OnClientDisconnectedCallback).AddTo(this);
        }

        public override void OnDestroy()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= onClientConnected.OnNext;
                NetworkManager.Singleton.OnClientDisconnectCallback -= onClientDisconnected.OnNext;
                NetworkManager.Singleton.ConnectionApprovalCallback -= OnApprovalCallback;
            }

            NetworkManager.OnInstantiated -= OnManagerInstantiatedCallback;
            NetworkManager.OnDestroying -= OnManagerDestroyCallback;

            networkSource?.TrySetCanceled();

            onClientConnected.Dispose();
            onClientDisconnected.Dispose();

            base.OnDestroy();
        }

        #endregion

        #region API

        public async UniTask<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (online.CurrentValue || networkSource != null)
            {
                Debug.LogWarning($"[{Tag}] Network is already up or connecting.");
                return false;
            }

            networkSource = new UniTaskCompletionSource<bool>();
            await using var registration = cancellationToken.Register(() => networkSource?.TrySetCanceled());

            try
            {
                if (NetworkManager.Singleton.StartClient())
                {
                    return await networkSource.Task.Timeout(Timeout);
                }

                Debug.LogError($"[{Tag}] StartClient failed immediately (check transport configuration).");
                return false;
            }
            catch (TimeoutException)
            {
                CleanupConnection();

                Debug.LogWarning($"[{Tag}] Connection timed out.");
                return false;
            }
            catch (OperationCanceledException)
            {
                CleanupConnection();

                Debug.LogWarning($"[{Tag}] Connection canceled.");
                return false;
            }
            finally
            {
                networkSource = null;
            }
        }

        public async UniTask<bool> HostAsync(CancellationToken cancellationToken = default)
        {
            if (online.CurrentValue || networkSource != null)
            {
                Debug.LogWarning($"[{Tag}] Network is already up or connecting.");
                return false;
            }

            networkSource = new UniTaskCompletionSource<bool>();
            await using var registration = cancellationToken.Register(() => networkSource?.TrySetCanceled());

            NetworkManager.Singleton.ConnectionApprovalCallback += OnApprovalCallback;

            try
            {
                if (NetworkManager.Singleton.StartHost())
                {
                    return await networkSource.Task.Timeout(Timeout);
                }

                NetworkManager.Singleton.ConnectionApprovalCallback -= OnApprovalCallback;

                Debug.LogError($"[{Tag}] StartHost failed immediately.");
                return false;
            }
            catch (TimeoutException)
            {
                CleanupConnection();

                Debug.LogWarning($"[{Tag}] Host startup timed out.");
                return false;
            }
            catch (OperationCanceledException)
            {
                CleanupConnection();

                Debug.LogWarning($"[{Tag}] Host startup canceled.");
                return false;
            }
            finally
            {
                networkSource = null;
            }
        }

        public void Disconnect()
        {
            if (networkSource != null)
            {
                networkSource.TrySetCanceled();
                networkSource = null;

                Debug.Log($"[{Tag}] Canceled pending connection attempts.");
            }

            CleanupConnection();

            online.Value = false;
        }

        #endregion

        #region Callbacks

        private void OnManagerInstantiatedCallback(NetworkManager instance)
        {
            instance.OnClientConnectedCallback += onClientConnected.OnNext;
            instance.OnClientDisconnectCallback += onClientDisconnected.OnNext;
        }

        private void OnManagerDestroyCallback(NetworkManager instance)
        {
            online.Value = false;

            if (instance)
            {
                instance.OnClientConnectedCallback -= onClientConnected.OnNext;
                instance.OnClientDisconnectCallback -= onClientDisconnected.OnNext;
                instance.ConnectionApprovalCallback -= OnApprovalCallback;
            }
        }

        private void OnClientConnectedCallback(ulong clientId)
        {
            if (!NetworkManager.Singleton || clientId != NetworkManager.Singleton.LocalClientId)
            {
                return;
            }

            networkSource?.TrySetResult(true);
            online.Value = true;

            if (NetworkManager.Singleton.IsServer)
            {
                foreach (var service in config.NetworkServices)
                {
                    var instance = Instantiate(service);

                    instance.Spawn();
                }
            }
        }

        private void OnClientDisconnectedCallback(ulong clientId)
        {
            if (!NetworkManager.Singleton || clientId != NetworkManager.Singleton.LocalClientId)
            {
                return;
            }

            networkSource?.TrySetResult(false);
            online.Value = false;
        }

        private void OnApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            foreach (var validator in config.ApprovalValidators)
            {
                if (!validator.Validate(request, out var reason))
                {
                    response.Approved = false;
                    response.Reason = reason;

                    return;
                }
            }

            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        #endregion

        #region Utils

        private void CleanupConnection()
        {
            if (!NetworkManager.Singleton)
            {
                return;
            }

            if (NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            NetworkManager.Singleton.ConnectionApprovalCallback -= OnApprovalCallback;
        }

        #endregion
    }
}