using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;

public class ServerStart : MonoBehaviour
{
    public static event System.Action ClientInstance;
    private const string _internalServerIP = "0.0.0.0";
    private string _externalServerIP = "0.0.0.0";
    private ushort _serverPort = 7777;
    private string _externalConnectionString => $"{_externalServerIP}:{_serverPort}";
    private IMultiplayService _multiplayServices;
    const int _multiplayServicesTimeout = 20000;
    private string _allocationID;
    private MultiplayEventCallbacks _serverCallBack;
    private IServerEvents _serverEvents;
    private BackfillTicket _localBackfillTicket;
    private CreateBackfillTicketOptions _createBackfillTicketOptions;
    private const int _ticketCheckMs = 1000;
    private MatchmakingResults _matchmakingPayload;
    private bool _backfilling = false;
    async void Start()
    {
        bool server = false;
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }
            if (args[i] == "-port" && (i + 1 < args.Length))
            {
                _serverPort = (ushort)int.Parse(args[i + 1]);
            }
            if (args[i] =="-ip" && (i + 1 < args.Length))
            {
                _externalServerIP = args[i + 1];
            }
        }

        if (server)
        {
            StartServer();
            await StartServerServices();
        }
        else
        {
            ClientInstance?.Invoke();
        }
    }

    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(_internalServerIP, _serverPort);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
    }

    async Task StartServerServices()
    {
        await UnityServices.InitializeAsync();
        try
        {
            _multiplayServices = MultiplayService.Instance;
            await _multiplayServices.StartServerQueryHandlerAsync((ushort)ConnectionApprovalHandler.MaxPlayers, "n/a", "n/a",
                "0", "n/a");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Something went wrong while starting the server services check the SQP: \n {e}");
        }
        try
        {
            _matchmakingPayload = await GetMatchmakerPayload(_multiplayServicesTimeout);
            if (_matchmakingPayload != null)
            {
                Debug.Log($"Got playload: {_matchmakingPayload}");
                await StartBackFill(_matchmakingPayload);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Something went wrong while starting the server services check the alocation & backfill: \n {e}");
        }
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload(int timeout)
    {
        var mathchmakerPayloadTask = SubcriseAndAwaitMatchmakerAllocation();
        if (await Task.WhenAny(mathchmakerPayloadTask, Task.Delay(timeout)) == mathchmakerPayloadTask)
        {
            return mathchmakerPayloadTask.Result;
        }

        return null;
    }

    private async Task<MatchmakingResults> SubcriseAndAwaitMatchmakerAllocation()
    {
        if (_multiplayServices == null)
        {
            return null;
        }

        _allocationID = null;
        _serverCallBack = new MultiplayEventCallbacks();
        _serverCallBack.Allocate += OnMultiplayAllocation;
        _serverEvents = await _multiplayServices.SubscribeToServerEventsAsync(_serverCallBack);
        _allocationID = await AwaitAllocationID();
        var mmPayload = await GetMatchmakerAllocationPayloadAsync();
        return mmPayload;
    }

   
    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"OnAllocation: {allocation.AllocationId}");
        if (string.IsNullOrEmpty(allocation.AllocationId))
        {
            return;
        }
        _allocationID = allocation.AllocationId;
    }

    private async Task<string> AwaitAllocationID()
    {
        var config = _multiplayServices.ServerConfig;
        Debug.Log($"Awaiting Allocation. Server Config is \n" +
                  $"-ServerId: {config.ServerId}\n" +
                  $"-AllocationID: {config.AllocationId}" + 
                  $"-Port: {config.Port}\n" + 
                  $"-QPort: {config.QueryPort}\n"+
                  $"logs: {config.ServerLogDirectory}");
        while (string.IsNullOrEmpty(_allocationID))
        {
            var configID = config.AllocationId;
            if (!string.IsNullOrEmpty(configID) && string.IsNullOrEmpty(_allocationID))
            {
                _allocationID = configID;
            }

            await Task.Delay(100);
        }

        return _allocationID;
    }
    private async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
    {
        try
        {
            var payloadAllocation =
                await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"{nameof(GetMatchmakerAllocationPayloadAsync)}:\n{modelAsJson}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Something went wrong in GetMatchmakerAllocationPayloadAsync: \n {e}");
        }

        return null;
    }
    
    private async Task StartBackFill(MatchmakingResults payload)
    {
        var backFillProperties = new BackfillTicketProperties(payload.MatchProperties);
        _localBackfillTicket = new BackfillTicket{Id =  payload.MatchProperties.BackfillTicketId, Properties = backFillProperties};
        await BeginBackfilling(payload);
    }

    private async Task BeginBackfilling(MatchmakingResults payload)
    {
     
        if (string.IsNullOrEmpty(_localBackfillTicket.Id))
        { 
            var matchProperties = payload.MatchProperties;
            _createBackfillTicketOptions = new CreateBackfillTicketOptions
                 {
                     Connection = _externalConnectionString,
                     QueueName = payload.QueueName,
                     Properties = new BackfillTicketProperties(matchProperties)
                 };
            _localBackfillTicket.Id =
                await MatchmakerService.Instance.CreateBackfillTicketAsync(_createBackfillTicketOptions);
        }
        _backfilling = true;
        #pragma warning disable 4014
        BackfillLoop();
        #pragma warning restore 4014
    }

    private async Task BackfillLoop()
    {
        while (_backfilling && NeedsPlayers())
        {
            _localBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(_localBackfillTicket.Id);
            if (!NeedsPlayers())
            {
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(_localBackfillTicket.Id);
                _localBackfillTicket.Id = null;
                _backfilling = false;
                return;
            }
            await Task.Delay(_ticketCheckMs);
        }

        _backfilling = false;
    }

    private void ClientDisconnected(ulong clientId)
    {
        if (!_backfilling && NetworkManager.Singleton.ConnectedClients.Count > 0 && NeedsPlayers())
        {
            BeginBackfilling(_matchmakingPayload);
        }
    }
    private bool NeedsPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < ConnectionApprovalHandler.MaxPlayers;
    }
    private void Dispose()
    {
        _serverCallBack.Allocate -= OnMultiplayAllocation;
        _serverEvents?.UnsubscribeAsync();
    }
}