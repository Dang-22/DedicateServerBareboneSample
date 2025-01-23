using Unity.Netcode;
using UnityEngine;

public class Lobby : NetworkBehaviour
{
    public void RequestSceneChange(string sceneName)
    {
        if (IsClient && !IsServer)
        {
            Debug.Log($"Client is requesting to change scene to: {sceneName}");
            RequestSceneChangeServerRpc(sceneName);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestSceneChangeServerRpc(string sceneName)
    {
        if (IsServer)
        {
            Debug.Log($"Server is loading scene: {sceneName}");
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("Only the server can change the scene.");
        }
    }
}