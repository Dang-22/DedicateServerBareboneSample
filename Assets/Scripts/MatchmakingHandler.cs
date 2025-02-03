using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class MatchmakingHandler
{
    private string _ticketId;
    public string TicketId => _ticketId;

    public async Task CreateBackfillTicket()
    {
        var results = await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
        Debug.Log($"Creating backfill ticket with Environment: {results.EnvironmentId} MatchId: {results.MatchId}");

        var backfillTicketProperties = new BackfillTicketProperties(results.MatchProperties);
        string queueName = "test";
        string connectionString = $"{MultiplayService.Instance.ServerConfig.IpAddress}:{MultiplayService.Instance.ServerConfig.Port}";

        var options = new CreateBackfillTicketOptions(
            queueName,
            connectionString,
            new Dictionary<string, object>(),
            backfillTicketProperties);

        Debug.Log("Requesting backfill ticket");
        _ticketId = await MatchmakerService.Instance.CreateBackfillTicketAsync(options);
        Debug.Log($"Created backfill ticket with ID: {_ticketId}");
    }

    public async Task ApproveBackfillTicket()
    {
        if (string.IsNullOrWhiteSpace(_ticketId))
        {
            Debug.Log("No backfill ticket to approve");
            return;
        }

        Debug.Log($"Doing backfill approval for _ticketId: {_ticketId}");
        await MatchmakerService.Instance.ApproveBackfillTicketAsync(_ticketId);
        Debug.Log($"Approved backfill ticket: {_ticketId}");
    }
} 