using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    private NetworkManager _networkManager;
    private UnityTransport _transport;

    void Awake()
    {
        _networkManager = NetworkManager.Singleton;
        _transport = _networkManager.GetComponent<UnityTransport>();
    }

    public void InitializeServer(int port)
    {
        _transport.SetConnectionData("0.0.0.0", (ushort)port);
        Debug.Log($"Network Transport {_transport.ConnectionData.Address}:{_transport.ConnectionData.Port}");

        if (!_networkManager.StartServer())
        {
            Debug.LogError("Failed to start server");
            throw new System.Exception("Failed to start server");
        }

        SetupNetworkCallbacks();
        LoadInitialScene();
    }

    private void SetupNetworkCallbacks()
    {
        _networkManager.OnClientConnectedCallback += OnClientConnected;
        _networkManager.OnServerStarted += OnServerStarted;
        _networkManager.OnServerStopped += OnServerStopped;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected");
    }

    private void OnServerStarted()
    {
        Debug.Log("Server started successfully");
        LoadInitialScene();
    }

    private void OnServerStopped(bool indicator)
    {
        Debug.Log($"Server stopped: {indicator}");
    }

    private void LoadInitialScene()
    {
        if (_networkManager.IsServer)
        {
            Debug.Log("Loading Level1 scene...");
            _networkManager.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        }
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
        {
            _networkManager.OnClientConnectedCallback -= OnClientConnected;
            _networkManager.OnServerStarted -= OnServerStarted;
            _networkManager.OnServerStopped -= OnServerStopped;
        }
    }
}