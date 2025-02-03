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
        DontDestroyOnLoad(gameObject);
        _networkManager = NetworkManager.Singleton;
        _transport = _networkManager.GetComponent<UnityTransport>();
    }

    public void InitializeServer(int port)
    {
        _transport.SetConnectionData("0.0.0.0",(ushort)port);
        Debug.Log($"Network Transport {_transport.ConnectionData.Address}:{_transport.ConnectionData.Port}");

        if (!_networkManager.StartServer())
        {
            Debug.LogError("Failed to start server");
            throw new Exception("Failed to start server");
        }

        SetupNetworkCallbacks();
        LoadInitialScene();
    }

    private void SetupNetworkCallbacks()
    {
        _networkManager.OnClientConnectedCallback += (clientId) => { Debug.Log("Client connected"); };
        _networkManager.OnServerStopped += (reason) => { Debug.Log("Server stopped"); };
    }

    private void LoadInitialScene()
    {
        _networkManager.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
    }
} 