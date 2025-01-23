using System;
using Unity.Netcode;
using UnityEngine;

public class ConnectionApprovalHandler : MonoBehaviour
{
    public static int MaxPlayers = 10;
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            Debug.Log($"ConnectionApprovalHandler initialized with MaxPlayers: {MaxPlayers}");
        }
        else
        {
            Debug.LogError("ConnectionApprovalHandler: NetworkManager.Singleton is null! Ensure NetworkManager exists in the scene.");
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true; 
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
        if (NetworkManager.Singleton.ConnectedClients.Count >= MaxPlayers)
        {
            response.Approved = false;
            response.Reason = "Server full";
        }

        response.Pending = false;
    }
}