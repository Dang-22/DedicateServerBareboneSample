using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : NetworkBehaviour
{
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked! Checking network...");

        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Network not started, starting as client...");
            NetworkManager.Singleton.StartClient();
        }

        Debug.Log("Sending ChangeSceneServerRpc...");
        ChangeSceneServerRpc("GamePlay");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSceneServerRpc(string sceneName)
    {
        Debug.Log($"Server received request to load scene: {sceneName}");

        if (!IsServer)
        {
            Debug.LogWarning("ChangeSceneServerRpc called, but we are not the server!");
            return;
        }
        
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        ChangeSceneClientRpc(sceneName);
    }

    [ClientRpc]
    private void ChangeSceneClientRpc(string sceneName)
    {
        Debug.Log($"Client is loading scene: {sceneName}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}