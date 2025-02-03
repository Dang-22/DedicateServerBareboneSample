using Unity.Netcode;
using UnityEngine;
using Unity.Networking.Transport;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour
{
    private ServerManager _serverManager;
    private MultiplayServiceHandler _multiplayHandler;
    private MatchmakingHandler _matchmakingHandler;
    private string backfillTicketId;
    
    [SerializeField] private float backfillInitialDelay = 4f;
    [SerializeField] private float backfillCheckInterval = 1f;

    private void Awake()
    {
        // Initialize components in Awake
        _serverManager = GetComponent<ServerManager>();
        if (_serverManager == null)
        {
            Debug.LogError("ServerManager component not found!");
            _serverManager = gameObject.AddComponent<ServerManager>();
        }
        
        _multiplayHandler = new MultiplayServiceHandler();
        _matchmakingHandler = new MatchmakingHandler();
    }

    async void Start()
    {
        StartCoroutine(InitializeServerCoroutine());
    }

    private IEnumerator InitializeServerCoroutine()
    {
        yield return new WaitForTask(InitializeServerAsync());
        
        // Wait for initial delay before starting backfill
        for (int i = (int)backfillInitialDelay; i >= 0; i--)
        {
            Debug.Log($"Waiting {i} seconds to start backfill");
            yield return new WaitForSeconds(1f);
        }

        // Start periodic backfill checks
        InvokeRepeating(nameof(ApproveBackfill), 0f, backfillCheckInterval);
    }

    private async Task InitializeServerAsync()
    {
        try
        {
            Debug.Log("Starting server initialization...");
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services initialized");
            
            if (_serverManager == null)
            {
                throw new Exception("ServerManager is null!");
            }
            
            var serverConfig = MultiplayService.Instance.ServerConfig;
            Debug.Log($"Got server config - Port: {serverConfig.Port}");
            
            _serverManager.InitializeServer(serverConfig.Port);
            Debug.Log("Server manager initialized");
            
            if (_multiplayHandler == null)
            {
                throw new Exception("MultiplayHandler is null!");
            }
            
            await _multiplayHandler.SubscribeToEvents();
            Debug.Log("Subscribed to multiplay events");
            
            if (!NetworkManager.Singleton.StartServer())
            {
                throw new Exception("Failed to start NetworkManager server!");
            }
            
            Debug.Log("Server started successfully");
            NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
            
            // Move backfill creation to after successful server start
            await CreateInitialBackfillTicket();
        }
        catch (Exception e)
        {
            Debug.LogError($"Server initialization failed: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            // Optionally, you might want to stop the server or application here
            // Application.Quit();
        }
    }

    private async Task CreateInitialBackfillTicket()
    {
        try
        {
            Debug.Log("Attempting to create initial backfill ticket...");
            await _matchmakingHandler.CreateBackfillTicket();
            backfillTicketId = _matchmakingHandler.TicketId;
            
            if (string.IsNullOrEmpty(backfillTicketId))
            {
                Debug.LogError("Initial backfill ticket was created but ID is null or empty!");
            }
            else
            {
                Debug.Log($"Successfully created initial backfill ticket: {backfillTicketId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create initial backfill ticket: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    private async void ApproveBackfill()
    {
        Debug.Log($"Current backfill ticket ID: {_matchmakingHandler?.TicketId}");
        
        if (_matchmakingHandler == null)
        {
            Debug.LogError("MatchmakingHandler is null!");
            return;
        }

        if (string.IsNullOrEmpty(_matchmakingHandler.TicketId))
        {
            Debug.LogWarning("No backfill ticket to approve - TicketId is null or empty");
            // Try to create a new ticket if we don't have one
            try
            {
                Debug.Log("Attempting to create new backfill ticket...");
                await _matchmakingHandler.CreateBackfillTicket();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create new backfill ticket: {e.Message}");
            }
            return;
        }

        try
        {
            await _matchmakingHandler.ApproveBackfillTicket();
            Debug.Log($"Successfully approved backfill ticket: {_matchmakingHandler.TicketId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in backfill approval: {e}");
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(ApproveBackfill));
    }
}

public class WaitForTask : CustomYieldInstruction
{
    private Task _task;
    
    public WaitForTask(Task task)
    {
        _task = task;
    }

    public override bool keepWaiting => !_task.IsCompleted;
}