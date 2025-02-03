using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour
{
    private ServerManager _serverManager;
    private MultiplayServiceHandler _multiplayHandler;
    private MatchmakingHandler _matchmakingHandler;
    
    [SerializeField] private float backfillInitialDelay = 4f;
    [SerializeField] private float backfillCheckInterval = 1f;

    void Start()
    {
        _serverManager = GetComponent<ServerManager>();
        _multiplayHandler = new MultiplayServiceHandler();
        _matchmakingHandler = new MatchmakingHandler();

        StartCoroutine(InitializeServerCoroutine());
        StartCoroutine(InitializeBackfill());
    }

    private IEnumerator InitializeServerCoroutine()
    {
        yield return new WaitForTask(InitializeServerAsync());
    }

    private async Task InitializeServerAsync()
    {
        await UnityServices.InitializeAsync();
        
        var serverConfig = MultiplayService.Instance.ServerConfig;
        _serverManager.InitializeServer(serverConfig.Port);
        
        await _multiplayHandler.SubscribeToEvents();
        await _matchmakingHandler.CreateBackfillTicket();
    }

    private IEnumerator InitializeBackfill()
    {
        // Initial delay countdown
        for (int i = (int)backfillInitialDelay; i >= 0; i--)
        {
            Debug.Log($"Waiting {i} seconds to start backfill");
            yield return new WaitForSeconds(1f);
        }

        // Start periodic backfill checks using InvokeRepeating
        InvokeRepeating(nameof(ApproveBackfill), 0f, backfillCheckInterval);
    }

    private async void ApproveBackfill()
    {
        try
        {
            await _matchmakingHandler.ApproveBackfillTicket();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in backfill approval: {e}");
            // Optional: Stop the repeating invocation if there's an error
            // CancelInvoke(nameof(ApproveBackfill));
        }
    }

    private void OnDestroy()
    {
        // Clean up the repeating invoke when the object is destroyed
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