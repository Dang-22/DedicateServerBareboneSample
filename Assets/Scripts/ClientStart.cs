using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEditor;
using UnityEngine;

public class ClientStart : MonoBehaviour
{
    private string _ticketId;

    private void OnEnable()
    {
        ServerStart.ClientInstance += SignIn;
    }

    private void OnDisable()
    {
        ServerStart.ClientInstance -= SignIn;
    }

    private async void SignIn()
    {
        await ClientSignIn("test");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task ClientSignIn(string serviceProfileName = null)
    {
        if (serviceProfileName != null)
        {
            #if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
            #endif
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(serviceProfileName);
            await UnityServices.InitializeAsync(initOptions);
        }
        else
        {
            await UnityServices.InitializeAsync();
        }
        Debug.Log($"Signed In Anonymously with PlayerId: {serviceProfileName}({PlayerID()})");
    }

    private string PlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }
#if UNITY_EDITOR
    private string GetCloneNumberSuffix()
    {
        return UnityEngine.Random.Range(0, 1000000).ToString();
    }
#endif
    public void StartClient()
    {
        CreateTicket();
    }

    private async void CreateTicket()
    {
        var options = new CreateTicketOptions("test");
        var players = new List<Player>
        {
            new Player(
                PlayerID(),
                new MatchmakingPlayerData
                {
                    Skill = 100
                }
                )
        };
        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        _ticketId = ticketResponse.Id;
        Debug.Log($"Ticket created ID: {_ticketId}");
        PollTicketStatus();
    }

    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssignment = false;
        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1f));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(_ticketId);
            if (ticketStatus == null)
            {
                continue;
            }
            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    gotAssignment = true;
                    TicketAssigned(multiplayAssignment);
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError($"Failed to get ticket status. Error: {multiplayAssignment.Message}");
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    gotAssignment = true;
                    Debug.LogError($"Failed to get ticket status. Tiket time out: {multiplayAssignment.Message}");
                    break;
                default:
                    throw new InvalidCastException();
            } 
        }while (!gotAssignment);
    }

    private void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log($"Ticket Assigned: {assignment.Ip}:{assignment.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        NetworkManager.Singleton.StartClient();
    }

    [Serializable]
    public class MatchmakingPlayerData
    {
        public int Skill;
    }
}